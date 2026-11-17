using HJJJJ.DeskReach;
using HJJJJ.DeskReach.Entities;
using HJJJJ.DeskReach.Plugins.Screen;
using HJJJJ.DeskReach.Plugins.Pointer;
using HJJJJ.DeskReach.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

internal static class TurboDeltaChecks
{
    public static readonly List<string> Passed = new List<string>();
    private static void Pass(string name) { Passed.Add(name); Console.WriteLine("PASS " + name); }
    private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    private static void Assert(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
    private static void Invalid(Action action)
    {
        try { action(); } catch (InvalidDataException) { return; }
        throw new InvalidOperationException("Malformed or dependent frame was accepted");
    }
    private static Bitmap Image(int width, int height)
    {
        var image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        using (var graphics = Graphics.FromImage(image))
        {
            graphics.Clear(Color.White);
            for (int y = 0; y < height; y += 24)
            {
                graphics.DrawLine(Pens.Gray, 0, y, width, y);
                graphics.DrawString("DeskReach 0123456789 desktop sample", SystemFonts.DefaultFont, Brushes.Black, 8, y + 2);
            }
        }
        return image;
    }
    private static void Paint(Bitmap image, Color color, Rectangle region)
    {
        using (var graphics = Graphics.FromImage(image))
        using (var brush = new SolidBrush(color)) graphics.FillRectangle(brush, region);
    }
    private static void CheckDelta()
    {
        using (var source = Image(509, 257))
        using (var encoder = new ScreenDeltaEncoder())
        using (var decoder = new ScreenFrameDecoder())
        {
            var key = encoder.Encode(source, 80);
            Assert(key.IsKeyFrame, "first frame was not a keyframe");
            byte[] original = key.GetBytes();
            using (var first = decoder.Apply(original))
            {
                Assert(first.Width == 509 && first.Height == 257, "odd dimensions or padded stride failed");
                Assert(encoder.Encode(source, 80) == null, "unchanged pixels generated an update");
                Paint(source, Color.Red, new Rectangle(508, 256, 1, 1));
                var delta = encoder.Encode(source, 80);
                Assert(!delta.IsKeyFrame && delta.BaseFrameId == key.FrameId && delta.Regions.Count == 1,
                    "small change did not produce one delta region");
                Assert(delta.Regions[0].X == 384 && delta.Regions[0].Y == 256 &&
                    delta.Regions[0].Width == 125 && delta.Regions[0].Height == 1, "edge tile bounds wrong");
                using (var next = decoder.Apply(delta.GetBytes()))
                {
                    var edge = next.GetPixel(508, 256);
                    Assert(edge.R - edge.G > 30, "edge pixel not reconstructed");
                    Assert(next.GetPixel(30, 30) == first.GetPixel(30, 30), "unchanged tile was modified");
                }
                Assert(decoder.Apply(delta.GetBytes()) == null, "duplicate frame was applied twice");
                Paint(source, Color.Blue, new Rectangle(10, 10, 80, 80));
                encoder.Encode(source, 80); // 模拟丢失一个差量。
                Paint(source, Color.Green, new Rectangle(140, 20, 80, 80));
                var missingBase = encoder.Encode(source, 80);
                Invalid(() => { using (var unused = decoder.Apply(missingBase.GetBytes())) { } });
                encoder.RequestKeyFrame();
                var recovery = encoder.Encode(source, 80);
                Assert(recovery.IsKeyFrame, "resync did not force a keyframe");
                using (var restored = decoder.Apply(recovery.GetBytes()))
                    Assert(restored.GetPixel(30, 30).B > 150 && restored.GetPixel(30, 30).R < 80 &&
                        restored.GetPixel(160, 40).G > 70 && restored.GetPixel(160, 40).R < 80, "keyframe recovery failed");
                Assert(encoder.Encode(source, 20).IsKeyFrame, "quality change did not refresh full picture");
                Paint(source, Color.Black, new Rectangle(0, 0, source.Width, source.Height));
                Assert(encoder.Encode(source, 20).IsKeyFrame, "large-area change did not fall back to full frame");
                using (var resized = Image(300, 201))
                    Assert(encoder.Encode(resized, 20).IsKeyFrame, "resolution change did not force keyframe");
            }
            // 协议拒绝溢出坐标、截断、多余字节和协议版本混用。
            var overflow = (byte[])original.Clone();
            Buffer.BlockCopy(BitConverter.GetBytes(int.MaxValue), 0, overflow, ScreenFrame.HeaderSize, 4);
            Invalid(() => ScreenFrame.Parse(overflow));
            Invalid(() => ScreenFrame.Parse(original.Take(original.Length - 1).ToArray()));
            Invalid(() => ScreenFrame.Parse(original.Concat(new byte[] { 0 }).ToArray()));
            var version = (byte[])original.Clone(); version[4] = 99;
            Invalid(() => ScreenFrame.Parse(version));
        }
        // 一个包内先写入有效块、再遇到损坏块：旧的已发布快照必须保持不变。
        using (var source = Image(512, 384))
        using (var encoder = new ScreenDeltaEncoder())
        using (var decoder = new ScreenFrameDecoder())
        using (var snapshot = decoder.Apply(encoder.Encode(source, 50).GetBytes()))
        {
            Color before = snapshot.GetPixel(25, 25);
            Paint(source, Color.Red, new Rectangle(10, 10, 40, 40));
            Paint(source, Color.Blue, new Rectangle(400, 280, 40, 40));
            var delta = encoder.Encode(source, 50);
            Assert(delta.Regions.Count == 2, "multi-region failure fixture is invalid");
            byte[] valid = delta.GetBytes();
            delta.Regions[1].Jpeg = new byte[] { 0xff, 0xd8, 0xff, 0xd9 };
            Invalid(() => { using (var unused = decoder.Apply(delta.GetBytes())) { } });
            Assert(snapshot.GetPixel(25, 25) == before, "failed packet mutated an already published snapshot");
            Invalid(() => { using (var unused = decoder.Apply(valid)) { } });
            encoder.RequestKeyFrame();
            using (var recovered = decoder.Apply(encoder.Encode(source, 50).GetBytes()))
                Assert(recovered.GetPixel(25, 25).G < 80, "atomic failure did not recover from keyframe");
        }
        Pass("delta suppression, edge tiles, pixel preservation, duplicates, resync, quality, resize, atomic failure and bounds");
    }

    private sealed class RecordingClient : Client
    {
        public readonly List<byte[]> Packets = new List<byte[]>();
        protected override void SendBytes(byte[] bytes) { lock (Packets) Packets.Add(bytes); }
        public ScreenPacket Last() { lock (Packets) return new ScreenPacket(new BasePackage(Packets.Last()).DetailData); }
    }
    private sealed class LayoutHost : MainFrom
    {
        protected override bool ShowWithoutActivation => true;
        public LayoutHost()
        {
            ShowInTaskbar = false;
            Opacity = 0;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(-30000, -30000);
        }
    }
    private sealed class ParsedCollector
    {
        public readonly List<byte[]> Frames = new List<byte[]>();
        public readonly ManualResetEvent Complete = new ManualResetEvent(false);
        public void OnParsed(object sender, object args)
        {
            lock (Frames)
            {
                var property = args.GetType().GetProperty("Data");
                Frames.Add((byte[])(property != null ? property.GetValue(args) : args.GetType().GetField("Data").GetValue(args)));
                if (Frames.Count >= 2) Complete.Set();
            }
        }
    }
    private static void CheckFragmentation()
    {
        var unpackerType = typeof(Client).Assembly.GetType("HJJJJ.DeskReach.PackageUnpacker");
        var unpacker = Activator.CreateInstance(unpackerType, true);
        var collector = new ParsedCollector();
        var evt = unpackerType.GetEvent("OnDataParsed");
        var parameters = evt.EventHandlerType.GetMethod("Invoke").GetParameters()
            .Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
        var callback = Expression.Lambda(evt.EventHandlerType,
            Expression.Call(Expression.Constant(collector), typeof(ParsedCollector).GetMethod("OnParsed"),
                Expression.Convert(parameters[0], typeof(object)), Expression.Convert(parameters[1], typeof(object))), parameters).Compile();
        evt.AddEventHandler(unpacker, callback);
        var input = unpackerType.GetMethod("Input");
        var first = new BasePackage("test", new byte[] { 10, 20, 30 }).GetBytes();
        var second = new BasePackage("test", Enumerable.Range(0, 200).Select(x => (byte)x).ToArray()).GetBytes();
        var combined = first.Concat(second).ToArray();
        for (int offset = 0; offset < combined.Length;)
        {
            int length = Math.Min(offset < 12 ? 1 : 17, combined.Length - offset);
            input.Invoke(unpacker, new object[] { combined.Skip(offset).Take(length).ToArray() });
            offset += length;
        }
        Assert(collector.Complete.WaitOne(2000), "fragmented TCP packet was lost");
        lock (collector.Frames)
            Assert(collector.Frames.Count == 2 && collector.Frames[0].SequenceEqual(first) && collector.Frames[1].SequenceEqual(second),
                "fragmented/coalesced packets did not round-trip");
        Pass("TCP unpacker preserves split headers, split bodies and coalesced packets");
    }
    private sealed class View : IScreenViewContext, IDisposable
    {
        public Rectangle ScreenViewBounds { get; set; }
        public readonly Bitmap Source = Image(512, 384);
        private readonly ScreenDeltaEncoder encoder = new ScreenDeltaEncoder();
        private readonly ScreenFrameDecoder decoder = new ScreenFrameDecoder();
        public Bitmap Last;
        public int CaptureCalls, KeyRequests, LastQuality;
        public byte[] GetImage(int quality) { CaptureCalls++; LastQuality = quality; return encoder.Encode(Source, quality)?.GetBytes(); }
        public void ShowImage(byte[] bytes) { var next = decoder.Apply(bytes); if (next != null) { Last?.Dispose(); Last = next; } }
        public void RequestKeyFrame() { KeyRequests++; encoder.RequestKeyFrame(); }
        public void Dispose() { Last?.Dispose(); encoder.Dispose(); decoder.Dispose(); Source.Dispose(); }
    }
    private static void CheckFlow()
    {
        using (var senderClient = new RecordingClient())
        using (var receiverClient = new RecordingClient())
        using (var senderView = new View())
        using (var receiverView = new View())
        using (var sender = new ScreenPlugin(senderView))
        using (var receiver = new ScreenPlugin(receiverView))
        {
            typeof(Client).GetField("Role", Hidden).SetValue(senderClient, ClientRoleType.ControlledEnd);
            typeof(Client).GetField("Role", Hidden).SetValue(receiverClient, ClientRoleType.ControlEnd);
            sender.RegInit(senderClient); receiver.RegInit(receiverClient);
            var send = typeof(ScreenPlugin).GetMethod("SendFrame", Hidden);
            var process = typeof(ScreenPlugin).GetMethod("ProcessFrame", Hidden);
            Action tick = () => send.Invoke(sender, new object[] { null, null });
            Action<byte[]> receive = bytes => process.Invoke(receiver, new object[] { bytes, CancellationToken.None });
            tick(); tick();
            Assert(senderView.CaptureCalls == 1 && senderClient.Packets.Count == 1, "sender ignored outstanding ACK");
            var wire = senderClient.Last();
            var frame = ScreenFrame.Parse(wire.Image);
            Assert(wire.Code == ScreenActionType.ImageFrame && wire.Image[0] == (byte)'D', "wire still wrapped in GZip");
            sender.RaiseDataReceived(null, new ScreenPacket(ScreenActionType.ACK, new byte[24]).GetBytes());
            tick(); Assert(senderView.CaptureCalls == 1, "wrong ACK released sender");
            receive(wire.Image);
            Assert(receiverClient.Last().Code == ScreenActionType.ACK && receiverView.Last != null, "receiver failed to rebuild and ACK");
            sender.RaiseDataReceived(null, receiverClient.Last().GetBytes());
            Paint(senderView.Source, Color.Red, new Rectangle(10, 10, 40, 40)); tick();
            wire = senderClient.Last();
            Assert(!ScreenFrame.Parse(wire.Image).IsKeyFrame, "second wire frame was not delta");
            receive(wire.Image);
            Assert(receiverView.Last.GetPixel(25, 25).R > 180 && receiverView.Last.GetPixel(25, 25).G < 80,
                "delta flow did not update remote canvas");
            // 损坏 JPEG：接收端只能请求关键帧，不能 ACK 部分更新。
            var corrupt = ScreenFrame.Parse(wire.Image); corrupt.FrameId++; corrupt.BaseFrameId++;
            corrupt.Regions[0].Jpeg = new byte[] { 0xff, 0xd8, 0xff, 0xd9 };
            receive(corrupt.GetBytes());
            Assert(receiverClient.Last().Code == ScreenActionType.RequestKeyFrame, "corrupt delta was acknowledged");
            sender.RaiseDataReceived(null, receiverClient.Last().GetBytes());
            tick(); Assert(ScreenFrame.Parse(senderClient.Last().Image).IsKeyFrame && senderView.KeyRequests == 1, "request did not recover full frame");
            receive(senderClient.Last().Image);
            Assert(receiverClient.Last().Code == ScreenActionType.ACK, "receiver did not recover after corrupt frame");
            // 模拟 ACK 超时，不实际等待 2 秒；过期 ACK 不能确认新的关键帧。
            var oldAck = receiverClient.Last();
            typeof(ScreenPlugin).GetField("sentAt", Hidden).SetValue(sender, Stopwatch.GetTimestamp() - Stopwatch.Frequency * 3L);
            tick();
            Assert(senderView.KeyRequests == 2 && ScreenFrame.Parse(senderClient.Last().Image).IsKeyFrame, "ACK timeout did not force keyframe");
            int calls = senderView.CaptureCalls;
            sender.RaiseDataReceived(null, oldAck.GetBytes()); tick();
            Assert(senderView.CaptureCalls == calls, "stale ACK acknowledged a newer frame");
        }
        Pass("raw protocol flow, matching ACK, one in flight, corrupt-frame request and timeout recovery");
    }

    private static void CheckCaptureUpdates()
    {
        using (var capture = new HJJJJ.DeskReach.Windows.WindowsAPIScreenCapture())
        using (var decoder = new ScreenFrameDecoder())
        {
            byte[] first = null;
            for (int i = 0; i < 30 && first == null; i++) { first = capture.CaptureUpdate(50); if (first == null) Thread.Sleep(10); }
            Assert(first != null && ScreenFrame.Parse(first).IsKeyFrame, "actual capture did not start with a keyframe");
            var normalSize = new Size(ScreenFrame.Parse(first).Width, ScreenFrame.Parse(first).Height);
            using (var result = decoder.Apply(first)) Assert(result.Width > 0, "actual keyframe did not reconstruct");
            capture.RequestKeyFrame();
            var forced = capture.CaptureUpdate(50);
            Assert(forced != null && ScreenFrame.Parse(forced).IsKeyFrame, "actual/cached desktop did not resync");
            using (var result = decoder.Apply(forced)) { }
            var quality = capture.CaptureUpdate(80);
            Assert(quality != null && ScreenFrame.Parse(quality).IsKeyFrame, "quality change did not refresh actual/cached desktop");
            using (var result = decoder.Apply(quality))
                Assert(result.Width / 2 == normalSize.Width && result.Height / 2 == normalSize.Height,
                    "High still sends half resolution instead of native desktop resolution");
            var best = capture.CaptureUpdate(100);
            Assert(best != null && ScreenFrame.Parse(best).IsKeyFrame, "Quality did not refresh a static desktop");
            using (var result = decoder.Apply(best))
                Assert(result.Width == ScreenFrame.Parse(quality).Width, "Quality changed native resolution");
            var normal = capture.CaptureUpdate(50);
            Assert(normal != null && ScreenFrame.Parse(normal).IsKeyFrame, "returning to Normal did not refresh");
            using (var result = decoder.Apply(normal)) Assert(result.Size == normalSize, "Normal did not restore half resolution");
            Pass("actual " + capture.CaptureBackend + " capture, cached quality refresh and native/half resolution transitions");
        }
    }

    private static void CheckViewerLayoutAndQuality()
    {
        using (var host = new LayoutHost())
        using (var client = new RecordingClient())
        using (var form = new MasterForm(client))
        using (var senderClient = new RecordingClient())
        using (var senderView = new View())
        using (var sender = new ScreenPlugin(senderView))
        {
            typeof(Client).GetField("Role", Hidden).SetValue(client, ClientRoleType.ControlEnd);
            typeof(Client).GetField("Role", Hidden).SetValue(senderClient, ClientRoleType.ControlledEnd);
            sender.RegInit(senderClient);
            host.Show(); // 屏幕外、无焦点窗口，让原生 TabControl 执行真实可见状态下的布局。
            var tabs = (TabControl)typeof(MainFrom).GetField("closableTabControl1", Hidden).GetValue(host);
            typeof(MainFrom).GetMethod("AddFormToPage", Hidden).Invoke(host, new object[] { form, "test" });
            var picture = (PictureBox)typeof(MasterForm).GetField("pictureBox", Hidden).GetValue(form);
            var original = tabs.Size;
            int rightMargin = tabs.Parent.ClientSize.Width - tabs.Right;
            int bottomMargin = tabs.Parent.ClientSize.Height - tabs.Bottom;
            host.ClientSize = new Size(host.ClientSize.Width + 320, host.ClientSize.Height + 180);
            host.PerformLayout(); Application.DoEvents();
            Assert(tabs.Width == original.Width + 320 && tabs.Height == original.Height + 180,
                "remote tab does not grow when host window grows");
            Assert(picture.Size == picture.Parent.ClientSize, "picture viewport does not fill resized remote tab");
            Assert(picture.SizeMode == PictureBoxSizeMode.Zoom, "picture still uses fixed-size/AutoSize display");
            // 不再发送帧，画面也必须随着窗口尺寸变化；同时验证实际绘制，而不仅检查 SizeMode。
            form.ShowImage(Image(800, 400)); Application.DoEvents();
            var source = picture.Image;
            var boundsMethod = typeof(MasterForm).GetMethod("GetDisplayedImageBounds", Hidden);
            var before = (Rectangle)boundsMethod.Invoke(form, null);
            host.ClientSize = new Size(host.ClientSize.Width + 240, host.ClientSize.Height + 120);
            host.PerformLayout(); Application.DoEvents();
            var after = (Rectangle)boundsMethod.Invoke(form, null);
            Assert(ReferenceEquals(source, picture.Image) && after.Width > before.Width && after.Height > before.Height,
                "static desktop does not resize without a new remote frame: " + before + " -> " + after +
                "; viewport=" + picture.ClientSize + "; host=" + host.ClientSize + "; form=" + form.ClientSize +
                "; same=" + ReferenceEquals(source, picture.Image));
            Assert(after.Width <= picture.ClientSize.Width && after.Height <= picture.ClientSize.Height,
                "fitted picture is clipped by viewport height");
            host.WindowState = FormWindowState.Maximized; Application.DoEvents();
            var panel = tabs.Parent;
            Assert(tabs.Right == panel.ClientSize.Width - rightMargin && tabs.Bottom == panel.ClientSize.Height - bottomMargin,
                "maximized host leaves a fixed-size remote tab");
            var maximized = (Rectangle)boundsMethod.Invoke(form, null);
            Assert(ReferenceEquals(source, picture.Image) && maximized.Width > 0 &&
                maximized.Right <= picture.ClientSize.Width && maximized.Bottom <= picture.ClientSize.Height,
                "maximizing static desktop loses or clips image");
            host.WindowState = FormWindowState.Normal; Application.DoEvents();

            var pointMethod = typeof(MasterForm).GetMethod("TryGetRemotePoint", Hidden);
            Action<Size> coordinates = size => {
                form.ShowImage(Image(size.Width, size.Height)); Application.DoEvents();
                var bounds = (Rectangle)boundsMethod.Invoke(form, null);
                var args = new object[] { new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2), Point.Empty, false };
                Assert((bool)pointMethod.Invoke(form, args) && Math.Abs(((Point)args[1]).X - 50) <= 1 &&
                    Math.Abs(((Point)args[1]).Y - 50) <= 1, "letterboxed image center maps to wrong remote coordinates");
                var center = (Point)args[0];
                typeof(Control).GetMethod("OnMouseDown", Hidden).Invoke(picture,
                    new object[] { new MouseEventArgs(MouseButtons.Left, 1, center.X, center.Y, 0) });
                var pointer = new PointerPacket(new BasePackage(client.Packets.Last()).DetailData);
                Assert(pointer.Code == PointerActionType.LeftDown && Math.Abs(pointer.X - 50) <= 1 && Math.Abs(pointer.Y - 50) <= 1,
                    "actual mouse handler did not use fitted-image coordinates");
                var outside = bounds.Left > 0 ? new Point(0, bounds.Top + bounds.Height / 2) : new Point(bounds.Left + bounds.Width / 2, 0);
                typeof(Control).GetMethod("OnMouseUp", Hidden).Invoke(picture,
                    new object[] { new MouseEventArgs(MouseButtons.Left, 1, outside.X, outside.Y, 0) });
                pointer = new PointerPacket(new BasePackage(client.Packets.Last()).DetailData);
                Assert(pointer.Code == PointerActionType.LeftUp && pointer.X >= 0 && pointer.X < 100 && pointer.Y >= 0 && pointer.Y < 100,
                    "releasing drag in letterbox did not release remote mouse within bounds");
                int packets = client.Packets.Count;
                typeof(Control).GetMethod("OnMouseDown", Hidden).Invoke(picture,
                    new object[] { new MouseEventArgs(MouseButtons.Left, 1, outside.X, outside.Y, 0) });
                Assert(client.Packets.Count == packets, "actual mouse handler forwards letterbox click");
                args = new object[] { outside, Point.Empty, false };
                Assert(!(bool)pointMethod.Invoke(form, args), "letterbox click reaches remote desktop");
                args[2] = true;
                Assert((bool)pointMethod.Invoke(form, args), "drag/release outside picture cannot be clamped");
                var mapped = (Point)args[1];
                Assert(mapped.X >= 0 && mapped.X < 100 && mapped.Y >= 0 && mapped.Y < 100, "clamped coordinates outside remote desktop");
                using (var painted = new Bitmap(picture.Width, picture.Height))
                {
                    picture.DrawToBitmap(painted, picture.ClientRectangle);
                    Assert(painted.GetPixel(bounds.Left + 10, bounds.Top + 10).ToArgb() != picture.BackColor.ToArgb(),
                        "Zoom picture was not painted in fitted region");
                }
            };
            coordinates(new Size(800, 200)); coordinates(new Size(200, 800));

            var tools = (ToolStrip)typeof(MasterForm).GetField("toolStrip", Hidden).GetValue(form);
            var combo = tools.Items.OfType<ToolStripComboBox>().Single(item => item.Items.Contains("Quality"));
            Assert(combo.SelectedIndex == 2, "quality menu default differs from encoder default");
            var tick = typeof(ScreenPlugin).GetMethod("SendFrame", Hidden);
            var expected = new[] { 1, 20, 50, 80, 100, 50 };
            var indexes = new[] { 0, 1, 2, 3, 4, 2 };
            for (int i = 0; i < indexes.Length; i++)
            {
                combo.SelectedIndex = indexes[i];
                var request = client.Last();
                Assert(request.Code == ScreenActionType.ImageQuality && request.ImageQuality == expected[i], "quality menu sends wrong value");
                sender.RaiseDataReceived(null, request.GetBytes());
                tick.Invoke(sender, new object[] { null, null });
                Assert(senderView.LastQuality == expected[i] && ScreenFrame.Parse(senderClient.Last().Image).IsKeyFrame,
                    "quality menu did not reach sender or did not force full refresh");
                var ack = ScreenFrame.ReadHeader(senderClient.Last().Image).GetAcknowledgement();
                sender.RaiseDataReceived(null, new ScreenPacket(ScreenActionType.ACK, ack).GetBytes());
            }
        }
        Pass("resized host/tab/static picture, wide/portrait painting and coordinates, all quality menu profiles round-trip");
    }

    public static void Run() { CheckViewerLayoutAndQuality(); CheckDelta(); CheckFlow(); CheckCaptureUpdates(); CheckFragmentation(); }
    private static object Time(Action action)
    {
        for (int i = 0; i < 5; i++) action();
        var times = new List<double>();
        for (int i = 0; i < 100; i++) { var watch = Stopwatch.StartNew(); action(); times.Add(watch.Elapsed.TotalMilliseconds); }
        times.Sort(); return new { MeanMs = Math.Round(times.Average(), 3), P95Ms = Math.Round(times[94], 3), Samples = times.Count };
    }
    public static object Measure()
    {
        using (var image = Image(1280, 720))
        using (var codec = new TurboJpegCodec())
        using (var encoder = new ScreenDeltaEncoder())
        using (var decoder = new ScreenFrameDecoder())
        {
            var jpegEncoder = ImageCodecInfo.GetImageEncoders().First(e => e.FormatID == ImageFormat.Jpeg.Guid);
            Func<byte[]> gdiEncode = () => {
                using (var parameters = new EncoderParameters(1))
                using (var stream = new MemoryStream()) {
                    parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);
                    image.Save(stream, jpegEncoder, parameters); return stream.ToArray();
                }
            };
            var gdiJpeg = gdiEncode(); var turboJpeg = codec.Encode(image, 50);
            object gdiEncodeTime = Time(() => gdiEncode());
            object turboEncodeTime = Time(() => codec.Encode(image, 50));
            var extension = typeof(MasterForm).Assembly.GetType("System.Drawing.BitmapExtendsion");
            var gdiDecode = (Func<byte[], Bitmap>)Delegate.CreateDelegate(typeof(Func<byte[], Bitmap>), extension.GetMethod("ToBitmap"));
            object gdiDecodeTime = Time(() => { using (var result = gdiDecode(turboJpeg)) { } });
            object turboDecodeTime = Time(() => { using (var result = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppRgb))
                codec.DecodeInto(turboJpeg, result, new Rectangle(0, 0, image.Width, image.Height)); });
            var first = encoder.Encode(image, 50);
            int keyBytes = first.GetBytes().Length;
            using (var rendered = decoder.Apply(first.GetBytes())) { }
            long deltaBytes = 0, fullBytes = 0;
            var deltaTimes = new List<double>(); var decodeTimes = new List<double>();
            for (int i = 0; i < 100; i++)
            {
                Paint(image, i % 2 == 0 ? Color.Red : Color.Blue, new Rectangle(16, 16, 48, 48));
                var watch = Stopwatch.StartNew();
                var update = encoder.Encode(image, 50); var bytes = update.GetBytes();
                deltaTimes.Add(watch.Elapsed.TotalMilliseconds);
                Assert(!update.IsKeyFrame && update.Regions.Count == 1, "small-change benchmark unexpectedly sent full frame");
                deltaBytes += bytes.Length; fullBytes += codec.Encode(image, 50).Length + 72;
                watch.Restart(); using (var rendered = decoder.Apply(bytes)) { }
                decodeTimes.Add(watch.Elapsed.TotalMilliseconds);
            }
            return new { Is64BitProcess = Environment.Is64BitProcess, Quality = 50, Subsampling = "4:2:0", Size = "1280x720",
                GdiJpegBytes = gdiJpeg.Length, TurboJpegBytes = turboJpeg.Length,
                GdiEncode = gdiEncodeTime, TurboEncode = turboEncodeTime, GdiDecodeToOwnedBitmap = gdiDecodeTime, TurboDecodeToOwnedBitmap = turboDecodeTime,
                InitialKeyFrameBytes = keyBytes, DeltaMeanBytes = Math.Round(deltaBytes / 100.0, 1),
                FullFrameMeanBytes = Math.Round(fullBytes / 100.0, 1), SavedBytesPercent = Math.Round((1 - (double)deltaBytes / fullBytes) * 100, 2),
                DeltaEncodeMeanMs = Math.Round(deltaTimes.Average(), 3), DeltaDecodeAndSnapshotMeanMs = Math.Round(decodeTimes.Average(), 3) };
        }
    }
}
