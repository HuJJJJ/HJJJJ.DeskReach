using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Screen.Windows
{
    public interface IScreenViewContext
    {
        /// <summary>
        /// 当前屏幕分辨率
        /// </summary>
        Rectangle ScreenViewBounds { get; set; }

        void ShowImage(Bitmap bitmap);

        void InitBitmap(int width, int height, PixelFormat format);
    }
}
