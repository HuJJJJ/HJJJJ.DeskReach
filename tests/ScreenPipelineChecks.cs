using HJJJJ.DeskReach;
using HJJJJ.DeskReach.Entities;
using HJJJJ.DeskReach.Plugins.Screen;
using HJJJJ.DeskReach.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

internal static class ScreenPipelineChecks
{
    [DllImport("user32.dll")] private static extern uint GetGuiResources(IntPtr process, uint flags);
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly List<string> Passed = new List<string>();

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Pass(string name)
    {
        Passed.Add(name);
        Console.WriteLine("PASS " + name);
    }

    private sealed class Sink : IScreenViewContext
    {
        public Rectangle ScreenViewBounds { get; set; }
        public int CaptureCalls, Active, Peak;
        public bool ThrowCapture;
        public readonly ManualResetEvent CaptureEntered = new ManualResetEvent(false);
        public readonly ManualResetEvent AllowCapture = new ManualResetEvent(false);
        public readonly ManualResetEvent FirstShown = new ManualResetEvent(false);
        public readonly ManualResetEvent AllowShow = new ManualResetEvent(false);
        public readonly ManualResetEvent SecondShown = new ManualResetEvent(false);
        public readonly List<int> Shown = new List<int>();
        public void RequestKeyFrame() { }
        public byte[] GetImage(int quality)
        {
            Interlocked.Increment(ref CaptureCalls);
            int active = Interlocked.Increment(ref Active);
            Peak = Math.Max(Peak, active);
            try
            {
                CaptureEntered.Set();
                Check(AllowCapture.WaitOne(5000), "capture gate timed out");
                if (ThrowCapture) { ThrowCapture = false; throw new InvalidOperationException("expected capture failure"); }
                return null; // 不进行真实网络发送。
            }
            finally { Interlocked.Decrement(ref Active); }
        }
        public void ShowImage(byte[] image)
        {
            int count;
            lock (Shown) { Shown.Add((int)ScreenFrame.ReadHeader(image).FrameId); count = Shown.Count; }
            if (count == 1)
            {
                FirstShown.Set();
                Check(AllowShow.WaitOne(5000), "display gate timed out");
            }
            else SecondShown.Set();
        }
    }

    private static readonly Guid testStream = Guid.NewGuid();
    private static byte[] Envelope(int value, byte[] jpeg = null)
    {
        var frame = new ScreenFrame { StreamId = testStream, FrameId = value, IsKeyFrame = true, Width = 1280, Height = 720 };
        frame.Regions.Add(new ScreenRegion { Width = 1280, Height = 720, Jpeg = jpeg ?? new byte[] { 0xff, 0xd8, 0xff, 0xd9 } });
        return frame.GetBytes();
    }

    private static void CheckSendGate()
    {
        var sink = new Sink();
        using (var client = new Client())
        using (var plugin = new ScreenPlugin(sink))
        {
            typeof(Client).GetField("Role", PrivateInstance).SetValue(client, ClientRoleType.ControlledEnd);
            plugin.RegInit(client);
            var send = typeof(ScreenPlugin).GetMethod("SendFrame", PrivateInstance);
            ThreadStart trigger = () => send.Invoke(plugin, new object[] { null, null });
            var first = new Thread(trigger);
            first.Start();
            Check(sink.CaptureEntered.WaitOne(2000), "first capture did not start");
            var others = Enumerable.Range(0, 16).Select(_ => new Thread(trigger)).ToArray();
            foreach (var thread in others) thread.Start();
            foreach (var thread in others) Check(thread.Join(2000), "busy trigger waited instead of skipping");
            Check(sink.CaptureCalls == 1 && sink.Peak == 1, "capture callbacks overlapped");
            sink.AllowCapture.Set();
            Check(first.Join(2000), "capture did not complete");
            trigger();
            Check(sink.CaptureCalls == 2, "next trigger could not acquire gate");
            sink.ThrowCapture = true;
            trigger(); trigger();
            Check(sink.CaptureCalls == 4, "exception did not release capture gate");
        }
        Pass("send gate skips overlapping triggers and releases after failure");
    }

    private static void CheckLatestFrame()
    {
        var sink = new Sink();
        using (var plugin = new ScreenPlugin(sink))
        {
            plugin.StartReceive(); plugin.StartReceive();
            plugin.EnqueueFrame(Envelope(1));
            Check(sink.FirstShown.WaitOne(2000), "receiver did not process first frame");
            for (int i = 2; i <= 1000; i++) plugin.EnqueueFrame(Envelope(i));
            sink.AllowShow.Set();
            Check(sink.SecondShown.WaitOne(2000), "receiver did not process latest frame");
            plugin.DisablePlugin();
            lock (sink.Shown)
                Check(sink.Shown.SequenceEqual(new[] { 1, 1000 }), "obsolete frames were displayed");
            plugin.EnqueueFrame(Envelope(1001));
            Check(typeof(ScreenPlugin).GetField("pendingFrame", PrivateInstance).GetValue(plugin) == null,
                "disabled receiver retained a frame");
            plugin.EnablePlugin(); plugin.StartReceive();
            sink.SecondShown.Reset();
            plugin.EnqueueFrame(Envelope(1002));
            Check(sink.SecondShown.WaitOne(2000), "receiver could not restart after disable");
        }
        Pass("receiver can replace obsolete keyframes, starts once and can stop/restart");
    }

    private static byte[] MakeJpeg(Color color)
    {
        using (var bitmap = new Bitmap(1280, 720))
        using (var graphics = Graphics.FromImage(bitmap))
        using (var stream = new MemoryStream())
        {
            graphics.Clear(color);
            bitmap.Save(stream, ImageFormat.Jpeg);
            return stream.ToArray();
        }
    }

    private static bool IsDisposed(Bitmap bitmap)
    {
        try { bitmap.GetPixel(0, 0); return false; }
        catch (ArgumentException) { return true; }
    }

    private static void CheckUi()
    {
        using (var client = new Client())
        {
            var form = new MasterForm(client);
            var handle = form.Handle; // 创建不可见窗体的消息句柄，不打开窗口。
            var pending = typeof(MasterForm).GetField("pendingScreenImage", PrivateInstance);
            var picture = (PictureBox)typeof(MasterForm).GetField("pictureBox", PrivateInstance).GetValue(form);
            byte[] red = MakeJpeg(Color.Red), blue = MakeJpeg(Color.Blue);
            var worker = new Thread(() => form.ShowImage(Envelope(1, red)));
            worker.Start(); Check(worker.Join(2000), "UI update blocked worker");
            var superseded = (Bitmap)pending.GetValue(form);
            form.ShowImage(Envelope(2, blue));
            Check(IsDisposed(superseded), "superseded pending image was not disposed");
            Application.DoEvents();
            var displayed = (Bitmap)picture.Image;
            Check(displayed != null && displayed.GetPixel(0, 0).B > 200, "latest UI frame was not shown");
            form.ShowImage(Envelope(3, red));
            Application.DoEvents();
            Check(IsDisposed(displayed), "replaced displayed image was not disposed");
            displayed = (Bitmap)picture.Image;
            form.ShowImage(Envelope(4, blue));
            var abandoned = (Bitmap)pending.GetValue(form);
            form.Dispose();
            Check(IsDisposed(displayed) && IsDisposed(abandoned), "form disposal leaked images");
            form.ShowImage(Envelope(5, red)); // 关闭后到达的数据应安全忽略。
            Check(pending.GetValue(form) == null, "closed form retained incoming image");
        }
        Pass("UI coalesces updates, marshals from worker and releases superseded/closed images");
    }

    private static uint GdiCount()
    {
        using (var process = Process.GetCurrentProcess()) return GetGuiResources(process.Handle, 0);
    }
    private static void FullGc() { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
    private static object Timing(IEnumerable<double> times)
    {
        double[] sorted = times.OrderBy(x => x).ToArray();
        return new { Samples = sorted.Length, MeanMs = sorted.Length == 0 ? 0 : Math.Round(sorted.Average(), 2),
            P95Ms = sorted.Length == 0 ? 0 : Math.Round(sorted[(int)((sorted.Length - 1) * .95)], 2) };
    }

    private static object MeasureCapture(bool dxgi, out byte[] jpeg)
    {
        var times = new List<double>();
        jpeg = null;
        string backend;
        uint before, after;
        int empty = 0;
        using (var capture = new HJJJJ.DeskReach.Windows.WindowsAPIScreenCapture(dxgi))
        {
            // 预热并分配复用缓冲区。
            for (int i = 0; i < 30 && jpeg == null; i++) { jpeg = capture.CaptureScreen(50); if (jpeg == null) Thread.Sleep(10); }
            Check(jpeg != null, "capture could not produce initial image");
            FullGc(); before = GdiCount();
            for (int i = 0; i < 500 && times.Count < 100; i++)
            {
                var watch = Stopwatch.StartNew();
                byte[] next = capture.CaptureScreen(50);
                double elapsed = watch.Elapsed.TotalMilliseconds;
                if (next == null) { empty++; Thread.Sleep(10); continue; }
                times.Add(elapsed); jpeg = next;
            }
            backend = capture.CaptureBackend;
            FullGc(); after = GdiCount();
            Check(after <= before + 2, "GDI object count grew across captures");
        }
        FullGc();
        Check(times.Count > 0, "no measured capture frames");
        using (var stream = new MemoryStream(jpeg))
        using (var bitmap = new Bitmap(stream))
        {
            Check(bitmap.Width > 0 && bitmap.Height > 0, "capture produced invalid JPEG");
            Pass("capture " + backend + " produces JPEG and has stable GDI resources");
            return new { Requested = dxgi ? "DXGI with fallback" : "GDI", ActualBackend = backend,
                ImageWidth = bitmap.Width, ImageHeight = bitmap.Height, Timing = Timing(times), EmptyPolls = empty,
                GdiBefore = before, GdiAfter = after, GdiAfterDispose = GdiCount() };
        }
    }

    private sealed class PaintProbe : RemoteScreenPictureBox
    {
        public void PaintFrame(Graphics graphics) { base.OnPaint(new PaintEventArgs(graphics, ClientRectangle)); }
    }
    private sealed class DefaultPaintProbe : PictureBox
    {
        public void PaintFrame(Graphics graphics) { base.OnPaint(new PaintEventArgs(graphics, ClientRectangle)); }
    }

    private static object MeasureDisplay(byte[] jpeg)
    {
        var times = new List<double>();
        var normalPaint = new List<double>();
        var optimizedPaint = new List<double>();
        // 使用 PictureBox 的实际 Zoom 绘制路径；目标是内存位图，不包含桌面合成或真实网络。
        using (var picture = new PaintProbe { Size = new Size(1478, 784), SizeMode = PictureBoxSizeMode.Zoom })
        using (var normal = new DefaultPaintProbe { Size = new Size(1478, 784), SizeMode = PictureBoxSizeMode.Zoom })
        using (var target = new Bitmap(1478, 784))
        using (var graphics = Graphics.FromImage(target))
        using (var decoder = new ScreenFrameDecoder())
        for (int i = 0; i < 105; i++)
        {
            byte[] frame = Envelope(i + 1, jpeg);
            var watch = Stopwatch.StartNew();
            using (var image = decoder.Apply(frame))
            {
                double decodeMs = watch.Elapsed.TotalMilliseconds;
                picture.Image = image;
                normal.Image = image;
                double a = 0, b = 0;
                Action fast = () => { watch.Restart(); picture.PaintFrame(graphics); a = watch.Elapsed.TotalMilliseconds; };
                Action standard = () => { watch.Restart(); normal.PaintFrame(graphics); b = watch.Elapsed.TotalMilliseconds; };
                // 同一张图交替顺序比较，避免将分开运行时的系统负载波动误认为优化收益。
                if (i % 2 == 0) { fast(); standard(); } else { standard(); fast(); }
                picture.Image = null; normal.Image = null;
                if (i >= 5) { times.Add(decodeMs + a); optimizedPaint.Add(a); normalPaint.Add(b); }
            }
        }
        Pass("TurboJPEG keyframe reconstruction and PictureBox.Zoom painting to 1478x784 viewport");
        return new { DecodeAndOptimizedZoomPaint = Timing(times), DefaultZoomPaint = Timing(normalPaint), OptimizedZoomPaint = Timing(optimizedPaint) };
    }

    [STAThread]
    public static int Main(string[] args)
    {
        try
        {
            CheckSendGate(); CheckLatestFrame(); CheckUi();
            TurboDeltaChecks.Run();
            if (args.Length > 1 && args[1] == "--checks-only") return 0;
            byte[] gdiJpeg, dxgiJpeg;
            object gdi = MeasureCapture(false, out gdiJpeg);
            object dxgi = MeasureCapture(true, out dxgiJpeg);
            object display = MeasureDisplay(dxgiJpeg);
            object report = new { PassedChecks = Passed, ProtocolChecks = TurboDeltaChecks.Passed,
                CodecAndDelta = TurboDeltaChecks.Measure(), Gdi = gdi, Dxgi = dxgi,
                DecodeAndZoomPaintAt1478x784 = display, IncludesNetwork = false,
                IncludesPictureBoxPaintingToBitmap = true, IncludesDesktopCompositing = false };
            var json = new JavaScriptSerializer().Serialize(report);
            File.WriteAllText(args[0], json);
            Console.WriteLine(json);
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
