using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using SharpGen.Runtime;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace HJJJJ.DeskReach.Windows
{
    /// <summary>主屏的 DXGI 桌面复制，由外层捕获器串行调用。</summary>
    internal sealed class DesktopDuplicationCapture : IDisposable
    {
        private const int WaitTimeout = unchecked((int)0x887A0027);
        private ID3D11Device device;
        private ID3D11DeviceContext context;
        private IDXGIOutputDuplication duplication;
        private ID3D11Texture2D stagingTexture;
        private Bitmap bitmap;
        internal Bitmap CurrentFrame => bitmap;

        public DesktopDuplicationCapture()
        {
            try
            {
                using (var factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>())
                {
                    for (int adapterIndex = 0; factory.EnumAdapters1(adapterIndex, out var adapter).Success; adapterIndex++)
                    using (adapter)
                    {
                        for (int outputIndex = 0; adapter.EnumOutputs(outputIndex, out var output).Success; outputIndex++)
                        using (output)
                        {
                            var description = output.Description;
                            if (!description.AttachedToDesktop || description.DeviceName != Screen.PrimaryScreen.DeviceName) continue;
                            // 旋转显示器暂由 GDI 接管，避免显示未旋转的 DXGI 表面。
                            if (description.Rotation != ModeRotation.Identity && description.Rotation != ModeRotation.Unspecified)
                                throw new NotSupportedException("旋转显示器使用 GDI 捕获。");
                            D3D11.D3D11CreateDevice(adapter, DriverType.Unknown, DeviceCreationFlags.BgraSupport,
                                null, out device, out context).CheckError();
                            using (var output1 = output.QueryInterface<IDXGIOutput1>())
                                duplication = output1.DuplicateOutput(device);
                            return;
                        }
                    }
                }
                throw new NotSupportedException("没有找到支持桌面复制的主屏输出。");
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// 返回本对象持有的位图，调用者编码后即可，不能持有或 Dispose。
        /// 桌面没有新帧时返回 null，避免重复编码同一幅画面。
        /// </summary>
        public Bitmap CaptureFrame()
        {
            IDXGIResource resource = null;
            try
            {
                duplication.AcquireNextFrame(0, out var frameInfo, out resource);
            }
            catch (SharpGenException ex) when (ex.ResultCode.Code == WaitTimeout)
            {
                resource?.Dispose();
                return null;
            }
            catch { resource?.Dispose(); throw; }
            try
            {
                using (resource)
                using (var texture = resource.QueryInterface<ID3D11Texture2D>())
                {
                    EnsureBuffers(texture.Description);
                    // GPU 表面 -> CPU 可读纹理；Vortice 参数顺序为目标、源。
                    context.CopyResource(stagingTexture, texture);
                    var mapped = context.Map(stagingTexture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
                    try { CopyRows(mapped); }
                    finally { context.Unmap(stagingTexture, 0); }
                }
                return bitmap;
            }
            finally
            {
                // 只有成功取得帧后才归还，异常路径也必须归还。
                duplication.ReleaseFrame();
            }
        }

        private void EnsureBuffers(Texture2DDescription description)
        {
            if (bitmap != null && bitmap.Width == description.Width && bitmap.Height == description.Height) return;
            if (description.Format != Format.B8G8R8A8_UNorm)
                throw new NotSupportedException("桌面纹理不是 BGRA8 格式。");
            stagingTexture?.Dispose(); stagingTexture = null;
            bitmap?.Dispose(); bitmap = null;
            description.Usage = ResourceUsage.Staging;
            description.BindFlags = BindFlags.None;
            description.CpuAccessFlags = CpuAccessFlags.Read;
            description.OptionFlags = ResourceOptionFlags.None;
            stagingTexture = device.CreateTexture2D(description, null);
            // 忽略桌面纹理的 alpha，防止部分驱动的零 alpha 导致黑图。
            bitmap = new Bitmap(description.Width, description.Height, PixelFormat.Format32bppRgb);
        }

        private unsafe void CopyRows(MappedSubresource mapped)
        {
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            try
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    // RowPitch 可能含对齐字节，必须逐行拷贝有效像素。
                    Buffer.MemoryCopy((byte*)mapped.DataPointer + y * mapped.RowPitch,
                        (byte*)data.Scan0 + y * data.Stride, Math.Abs(data.Stride), bitmap.Width * 4);
                }
            }
            finally { bitmap.UnlockBits(data); }
        }

        public void Dispose()
        {
            bitmap?.Dispose(); bitmap = null;
            stagingTexture?.Dispose(); stagingTexture = null;
            duplication?.Dispose(); duplication = null;
            context?.Dispose(); context = null;
            device?.Dispose(); device = null;
        }
    }
}
