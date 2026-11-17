using SharpGen.Runtime;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HJJJJ.DeskReach.Windows
{
    /// <summary>优先 DXGI，失败时回退 GDI。每个会话持有一个实例，复用图像缓冲区。</summary>
    public sealed class WindowsAPIScreenCapture : IDisposable
    {
        private const int DesktopHorzRes = 118;
        private const int DesktopVertRes = 117;
        private readonly object captureLock = new object();
        private readonly bool useDesktopDuplication;
        private readonly TurboJpegCodec codec = new TurboJpegCodec();
        private readonly ScreenDeltaEncoder deltaEncoder = new ScreenDeltaEncoder();
        private DesktopDuplicationCapture duplication;
        private DateTime retryDuplicationAt;
        private Bitmap desktopBitmap;
        private Graphics desktopGraphics;
        private Bitmap scaledBitmap;
        private Graphics scaledGraphics;
        private bool disposed;

        public string CaptureBackend { get; private set; } = "Not initialized";

        public WindowsAPIScreenCapture(bool useDesktopDuplication = true)
        {
            this.useDesktopDuplication = useDesktopDuplication;
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateDC(string driver, string device, string output, IntPtr data);
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteDC(IntPtr dc);
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool BitBlt(IntPtr target, int x, int y, int width, int height,
            IntPtr source, int sourceX, int sourceY, int operation);
        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr dc, int index);

        public static double GetScreenScalingFactor()
        {
            using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr dc = graphics.GetHdc();
                try
                {
                    return (double)GetDeviceCaps(dc, DesktopVertRes) / Screen.PrimaryScreen.Bounds.Height;
                }
                finally { graphics.ReleaseHdc(dc); }
            }
        }

        public byte[] CaptureScreen(int quality)
        {
            lock (captureLock)
            {
                if (disposed) throw new ObjectDisposedException(nameof(WindowsAPIScreenCapture));
                var frame = CaptureRawFrame(false);
                return frame == null ? null : codec.Encode(ScaleFrame(frame, quality), quality);
            }
        }

        public byte[] CaptureUpdate(int quality)
        {
            lock (captureLock)
            {
                if (disposed) throw new ObjectDisposedException(nameof(WindowsAPIScreenCapture));
                bool allowCached = deltaEncoder.NeedsKeyFrame || deltaEncoder.Quality != Math.Max(1, Math.Min(100, quality));
                var frame = CaptureRawFrame(allowCached);
                if (frame == null) return null;
                return deltaEncoder.Encode(ScaleFrame(frame, quality), quality)?.GetBytes();
            }
        }

        public void RequestKeyFrame()
        {
            lock (captureLock)
            {
                if (!disposed) deltaEncoder.RequestKeyFrame();
            }
        }

        private Bitmap CaptureRawFrame(bool allowCached)
        {
            Bitmap frame = null;
            if (useDesktopDuplication && DateTime.UtcNow >= retryDuplicationAt)
            {
                try
                {
                    if (duplication == null) duplication = new DesktopDuplicationCapture();
                    frame = duplication.CaptureFrame();
                    CaptureBackend = "DXGI";
                    if (frame == null) return allowCached ? duplication.CurrentFrame : null;
                }
                catch (Exception ex) when (ex is SharpGenException || ex is NotSupportedException ||
                    ex is DllNotFoundException || ex is EntryPointNotFoundException)
                {
                    duplication?.Dispose(); duplication = null;
                    retryDuplicationAt = DateTime.UtcNow.AddSeconds(5);
                    if (CaptureBackend != "GDI") Console.WriteLine("DXGI 暂不可用，回退 GDI: " + ex.Message);
                }
            }
            if (frame == null)
            {
                frame = CaptureGdiFrame();
                CaptureBackend = "GDI";
            }
            return frame;
        }

        private Bitmap CaptureGdiFrame()
        {
            IntPtr source = CreateDC("DISPLAY", Screen.PrimaryScreen.DeviceName, null, IntPtr.Zero);
            if (source == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
            try
            {
                int width = GetDeviceCaps(source, DesktopHorzRes);
                int height = GetDeviceCaps(source, DesktopVertRes);
                if (desktopBitmap == null || desktopBitmap.Width != width || desktopBitmap.Height != height)
                {
                    desktopGraphics?.Dispose(); desktopGraphics = null;
                    desktopBitmap?.Dispose(); desktopBitmap = null;
                    desktopBitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);
                    desktopGraphics = Graphics.FromImage(desktopBitmap);
                }
                IntPtr target = desktopGraphics.GetHdc();
                try
                {
                    if (!BitBlt(target, 0, 0, width, height, source, 0, 0, 0x00CC0020))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                finally { desktopGraphics.ReleaseHdc(target); }
                return desktopBitmap;
            }
            finally
            {
                // CreateDC 对应 DeleteDC；Graphics.GetHdc 对应 ReleaseHdc，不能混用。
                DeleteDC(source);
            }
        }

        private Bitmap ScaleFrame(Bitmap frame, int quality)
        {
            // High / Quality 保留原始像素；其余档位继续使用半分辨率，保留默认性能。
            int divisor = quality >= 80 ? 1 : 2;
            int width = Math.Max(1, frame.Width / divisor);
            int height = Math.Max(1, frame.Height / divisor);
            if (scaledBitmap == null || scaledBitmap.Width != width || scaledBitmap.Height != height)
            {
                scaledGraphics?.Dispose(); scaledGraphics = null;
                scaledBitmap?.Dispose(); scaledBitmap = null;
                scaledBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                scaledGraphics = Graphics.FromImage(scaledBitmap);
                scaledGraphics.CompositingMode = CompositingMode.SourceCopy;
                scaledGraphics.InterpolationMode = InterpolationMode.Bilinear;
                scaledGraphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            }
            // 快速缩放并复用目标缓冲区，替代每帧 GetThumbnailImage。
            if (divisor == 1) scaledGraphics.DrawImageUnscaled(frame, 0, 0);
            else scaledGraphics.DrawImage(frame, new Rectangle(0, 0, width, height),
                0, 0, frame.Width, frame.Height, GraphicsUnit.Pixel);
            return scaledBitmap;
        }

        public void Dispose()
        {
            lock (captureLock)
            {
                if (disposed) return;
                disposed = true;
                codec.Dispose(); deltaEncoder.Dispose();
                duplication?.Dispose(); duplication = null;
                scaledGraphics?.Dispose(); scaledGraphics = null;
                scaledBitmap?.Dispose(); scaledBitmap = null;
                desktopGraphics?.Dispose(); desktopGraphics = null;
                desktopBitmap?.Dispose(); desktopBitmap = null;
            }
        }
    }
}
