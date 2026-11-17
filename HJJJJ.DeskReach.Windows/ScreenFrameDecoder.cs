using HJJJJ.DeskReach.Plugins.Screen;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace HJJJJ.DeskReach.Windows
{
    /// <summary>顺序重建差量帧。交给 UI 的快照独立于重建缓冲区，可以安全合并显示。</summary>
    public sealed class ScreenFrameDecoder : IDisposable
    {
        private readonly object decoderLock = new object();
        private readonly TurboJpegCodec codec = new TurboJpegCodec();
        private Bitmap canvas;
        private Guid streamId;
        private long frameId;
        private bool disposed;

        public Bitmap Apply(byte[] bytes)
        {
            var frame = ScreenFrame.Parse(bytes);
            lock (decoderLock)
            {
                if (disposed) throw new ObjectDisposedException(nameof(ScreenFrameDecoder));
                if (frame.StreamId == streamId && frame.FrameId <= frameId) return null;
                if (!frame.IsKeyFrame && (canvas == null || frame.StreamId != streamId || frame.BaseFrameId != frameId ||
                    frame.Width != canvas.Width || frame.Height != canvas.Height))
                    throw new InvalidDataException("差量基准缺失，需要关键帧。");
                try
                {
                    if (frame.IsKeyFrame)
                    {
                        canvas?.Dispose(); canvas = null;
                        canvas = new Bitmap(frame.Width, frame.Height, PixelFormat.Format32bppRgb);
                    }
                    foreach (var region in frame.Regions)
                        codec.DecodeInto(region.Jpeg, canvas, new Rectangle(region.X, region.Y, region.Width, region.Height));
                    var snapshot = CopyBitmap(canvas);
                    streamId = frame.StreamId; frameId = frame.FrameId;
                    return snapshot;
                }
                catch
                {
                    // 中途失败不发布半张画面，也不能继续以部分写入的缓冲区做基准。
                    canvas?.Dispose(); canvas = null; frameId = 0; streamId = Guid.Empty;
                    throw;
                }
            }
        }

        private static unsafe Bitmap CopyBitmap(Bitmap source)
        {
            var copy = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppRgb);
            try
            {
                var bounds = new Rectangle(0, 0, source.Width, source.Height);
                var read = source.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                try
                {
                    var write = copy.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                    try
                    {
                        for (int y = 0; y < source.Height; y++)
                            Buffer.MemoryCopy((byte*)read.Scan0 + y * read.Stride, (byte*)write.Scan0 + y * write.Stride,
                                Math.Abs(write.Stride), source.Width * 4);
                    }
                    finally { copy.UnlockBits(write); }
                }
                finally { source.UnlockBits(read); }
                return copy;
            }
            catch { copy.Dispose(); throw; }
        }

        public void Dispose()
        {
            lock (decoderLock)
            {
                if (disposed) return;
                disposed = true; canvas?.Dispose(); canvas = null; codec.Dispose();
            }
        }
    }
}
