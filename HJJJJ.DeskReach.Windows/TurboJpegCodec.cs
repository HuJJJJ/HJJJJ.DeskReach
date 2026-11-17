using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace HJJJJ.DeskReach.Windows
{
    /// <summary>libjpeg-turbo 3.2，直接读取/写入位图像素，无 GDI+ JPEG 中转。</summary>
    public sealed class TurboJpegCodec : IDisposable
    {
        private readonly object codecLock = new object();
        private JpegHandle compressor;
        private JpegHandle decompressor;
        private bool disposed;
        private JpegHandle GetHandle(bool compress)
        {
            if (disposed) throw new ObjectDisposedException(nameof(TurboJpegCodec));
            var handle = compress ? compressor : decompressor;
            if (handle == null)
            {
                handle = new JpegHandle(Native.tj3InitVersion(compress ? 0 : 1, 3002000));
                if (handle.IsInvalid) throw new InvalidOperationException("TurboJPEG 初始化失败。");
                Check(Native.tj3Set(handle, 0, 1), handle); // 损坏 JPEG 的 warning 也视为失败。
                if (compress) compressor = handle; else decompressor = handle;
            }
            return handle;
        }

        private static void Check(int result, JpegHandle handle)
        {
            if (result != 0) throw new InvalidDataException("TurboJPEG: " + Marshal.PtrToStringAnsi(Native.tj3GetErrorStr(handle)));
        }

        internal byte[] EncodePixels(IntPtr pixels, int width, int height, int pitch, int quality)
        {
            if (pixels == IntPtr.Zero || width <= 0 || height <= 0 || width > 8192 || height > 8192 ||
                (long)width * height > HJJJJ.DeskReach.Plugins.Screen.ScreenFrame.MaxPixels || pitch < width * 3)
                throw new ArgumentException("JPEG 像素缓冲区或行跨度无效。");
            lock (codecLock)
            {
                var handle = GetHandle(true);
                Check(Native.tj3Set(handle, 3, Math.Max(1, Math.Min(100, quality))), handle);
                Check(Native.tj3Set(handle, 4, 2), handle); // 4:2:0，与原 GDI+ 默认 JPEG 保持一致。
                IntPtr jpeg = IntPtr.Zero;
                UIntPtr size = UIntPtr.Zero;
                try
                {
                    Check(Native.tj3Compress8(handle, pixels, width, pitch, height, 1, ref jpeg, ref size), handle); // BGR24
                    ulong length = size.ToUInt64();
                    if (length == 0 || length > HJJJJ.DeskReach.Plugins.Screen.ScreenFrame.MaxFrameBytes)
                        throw new InvalidDataException("JPEG 输出长度无效。");
                    var bytes = new byte[(int)length];
                    Marshal.Copy(jpeg, bytes, 0, bytes.Length);
                    return bytes;
                }
                finally { if (jpeg != IntPtr.Zero) Native.tj3Free(jpeg); }
            }
        }

        public byte[] Encode(Bitmap bitmap, int quality)
        {
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try { return EncodePixels(data.Scan0, bitmap.Width, bitmap.Height, data.Stride, quality); }
            finally { bitmap.UnlockBits(data); }
        }

        public void DecodeInto(byte[] jpeg, Bitmap bitmap, Rectangle region)
        {
            if (jpeg == null || jpeg.Length == 0 || jpeg.Length > HJJJJ.DeskReach.Plugins.Screen.ScreenFrame.MaxFrameBytes)
                throw new InvalidDataException("JPEG 输入长度无效。");
            lock (codecLock)
            {
                var handle = GetHandle(false);
                Check(Native.tj3DecompressHeader(handle, jpeg, new UIntPtr((uint)jpeg.Length)), handle);
                if (Native.tj3Get(handle, 5) != region.Width || Native.tj3Get(handle, 6) != region.Height ||
                    region.X < 0 || region.Y < 0 || region.Width <= 0 || region.Height <= 0 ||
                    region.Width > bitmap.Width || region.Height > bitmap.Height ||
                    region.X > bitmap.Width - region.Width || region.Y > bitmap.Height - region.Height)
                    throw new InvalidDataException("JPEG 尺寸与更新区域不符。");
                var data = bitmap.LockBits(region, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                try
                {
                    if (data.Stride < region.Width * 4) throw new InvalidDataException("目标像素行跨度无效。");
                    Check(Native.tj3Decompress8(handle, jpeg, new UIntPtr((uint)jpeg.Length), data.Scan0, data.Stride, 3), handle); // BGRX32
                }
                finally { bitmap.UnlockBits(data); }
            }
        }

        public void Dispose()
        {
            lock (codecLock)
            {
                if (disposed) return;
                disposed = true;
                compressor?.Dispose(); decompressor?.Dispose();
            }
        }

        private sealed class JpegHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public JpegHandle(IntPtr value) : base(true) { SetHandle(value); }
            protected override bool ReleaseHandle() { Native.tj3Destroy(handle); return true; }
        }

        private static class Native
        {
            // 保持模块加载到进程结束；SafeHandle 仍会释放各编解码上下文。
            private static readonly IntPtr module;
            static Native()
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes",
                    Environment.Is64BitProcess ? "win-x64" : "win-x86", "native", "turbojpeg.dll");
                module = LoadLibraryEx(path, IntPtr.Zero, 8);
                if (module == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error(), "无法加载 TurboJPEG: " + path);
            }
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern IntPtr LoadLibraryEx(string path, IntPtr reserved, int flags);
            [DllImport("turbojpeg.dll", CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr tj3InitVersion(int type, int version);
            [DllImport("turbojpeg.dll", CallingConvention = CallingConvention.Cdecl)] public static extern void tj3Destroy(IntPtr handle);
            [DllImport("turbojpeg.dll", CallingConvention = CallingConvention.Cdecl)] public static extern int tj3Set(JpegHandle handle, int parameter, int value);
            [DllImport("turbojpeg.dll", CallingConvention = CallingConvention.Cdecl)] public static extern int tj3Get(JpegHandle handle, int parameter);
            [DllImport("turbojpeg.dll", CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr tj3GetErrorStr(JpegHandle handle);
            [DllImport("turbojpeg.dll", CallingConvention = CallingConvention.Cdecl)] public static extern int tj3Compress8(JpegHandle handle, IntPtr pixels,
                int width, int pitch, int height, int format, ref IntPtr jpeg, ref UIntPtr size);
            [DllImport("turbojpeg.dll", CallingConvention = CallingConvention.Cdecl)] public static extern int tj3DecompressHeader(JpegHandle handle, byte[] jpeg, UIntPtr size);
            [DllImport("turbojpeg.dll", CallingConvention = CallingConvention.Cdecl)] public static extern int tj3Decompress8(JpegHandle handle, byte[] jpeg,
                UIntPtr size, IntPtr pixels, int pitch, int format);
            [DllImport("turbojpeg.dll", CallingConvention = CallingConvention.Cdecl)] public static extern void tj3Free(IntPtr memory);
        }
    }
}
