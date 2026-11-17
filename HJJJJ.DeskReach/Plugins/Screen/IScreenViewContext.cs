using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Screen
{
    public interface IScreenViewContext
    {
        /// <summary>
        /// 当前屏幕分辨率
        /// </summary>
        Rectangle ScreenViewBounds { get; set; }

        // 传递 ScreenFrame v2（完整 JPEG 关键帧或 JPEG 区域更新），不再是 GZip 数据。
        byte[] GetImage(int quality);
        void ShowImage(byte[] image);
        void RequestKeyFrame();


    }
}
