using HJJJJ.DeskReach.Plugins.Screen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace HJJJJ.DeskReach.Windows
{
    /// <summary>在缩放后的原始像素上比较 128×128 块，仅编码变化区域。</summary>
    public sealed class ScreenDeltaEncoder : IDisposable
    {
        public const int TileSize = 128;
        private readonly Guid streamId = Guid.NewGuid();
        private readonly TurboJpegCodec codec = new TurboJpegCodec();
        private readonly Stopwatch keyFrameClock = Stopwatch.StartNew();
        private byte[] previousPixels;
        private int previousWidth, previousHeight, previousStride;
        private int previousQuality = -1;
        private long frameId;
        private bool forceKeyFrame = true;
        private bool disposed;
        public bool NeedsKeyFrame => forceKeyFrame;
        public int Quality => previousQuality;
        public void RequestKeyFrame() { forceKeyFrame = true; }

        public unsafe ScreenFrame Encode(Bitmap image, int quality)
        {
            if (disposed) throw new ObjectDisposedException(nameof(ScreenDeltaEncoder));
            quality = Math.Max(1, Math.Min(100, quality));
            int width = image.Width, height = image.Height;
            if (width <= 0 || height <= 0 || width > 8192 || height > 8192 || (long)width * height > ScreenFrame.MaxPixels)
                throw new ArgumentOutOfRangeException(nameof(image));
            var data = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                if (data.Stride < width * 3) throw new ArgumentException("像素行跨度无效。");
                bool key = forceKeyFrame || previousPixels == null || width != previousWidth || height != previousHeight ||
                    data.Stride != previousStride || quality != previousQuality || keyFrameClock.ElapsedMilliseconds >= 5000;
                var changed = new List<Rectangle>();
                long changedPixels = 0;
                if (!key)
                {
                    fixed (byte* previous = previousPixels)
                    {
                        for (int y = 0; y < height; y += TileSize)
                        for (int x = 0; x < width; x += TileSize)
                        {
                            var region = new Rectangle(x, y, Math.Min(TileSize, width - x), Math.Min(TileSize, height - y));
                            if (!IsChanged((byte*)data.Scan0, data.Stride, previous, region)) continue;
                            changed.Add(region);
                            changedPixels += (long)region.Width * region.Height;
                        }
                    }
                    if (changed.Count == 0) return null;
                    // 大面积变化直接发一张全图，避免大量小 JPEG 的头部和调用开销。
                    key = changedPixels * 100 >= (long)width * height * 60 || changed.Count > 256;
                }
                if (key) { changed.Clear(); changed.Add(new Rectangle(0, 0, width, height)); }
                var frame = new ScreenFrame { StreamId = streamId, FrameId = frameId + 1, BaseFrameId = key ? 0 : frameId,
                    IsKeyFrame = key, Width = width, Height = height };
                foreach (var region in changed)
                {
                    IntPtr pixels = IntPtr.Add(data.Scan0, region.Y * data.Stride + region.X * 3);
                    frame.Regions.Add(new ScreenRegion { X = region.X, Y = region.Y, Width = region.Width, Height = region.Height,
                        Jpeg = codec.EncodePixels(pixels, region.Width, region.Height, data.Stride, quality) });
                }
                // 只有全部区域编码成功后才推进基准，失败后下一次仍可重试同一状态。
                int length = checked(data.Stride * height);
                if (previousPixels == null || previousPixels.Length != length) previousPixels = new byte[length];
                Marshal.Copy(data.Scan0, previousPixels, 0, length);
                previousWidth = width; previousHeight = height; previousStride = data.Stride; previousQuality = quality;
                frameId = frame.FrameId; forceKeyFrame = false;
                if (key) keyFrameClock.Restart();
                return frame;
            }
            finally { image.UnlockBits(data); }
        }

        private static unsafe bool IsChanged(byte* current, int stride, byte* previous, Rectangle region)
        {
            int rowBytes = region.Width * 3;
            for (int y = region.Y; y < region.Bottom; y++)
            {
                byte* left = current + y * stride + region.X * 3;
                byte* right = previous + y * stride + region.X * 3;
                int i = 0;
                for (; i + 8 <= rowBytes; i += 8) if (*(ulong*)(left + i) != *(ulong*)(right + i)) return true;
                for (; i < rowBytes; i++) if (left[i] != right[i]) return true;
            }
            return false;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true; codec.Dispose(); previousPixels = null;
        }
    }
}
