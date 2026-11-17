using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HJJJJ.DeskReach.Windows
{
    /// <summary>沿用 PictureBox.Zoom 的布局，使用屏幕画面适合的绘制参数。</summary>
    public class RemoteScreenPictureBox : PictureBox
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            var state = e.Graphics.Save();
            try
            {
                // 本机默认 Bilinear 缩放较慢；沿用上一轮已实测较快的参数。
                e.Graphics.CompositingMode = CompositingMode.SourceCopy;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                base.OnPaint(e);
            }
            finally { e.Graphics.Restore(state); }
        }
    }
}
