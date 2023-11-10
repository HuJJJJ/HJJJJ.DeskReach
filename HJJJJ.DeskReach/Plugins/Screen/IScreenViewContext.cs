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
        Rectangle Bounds { get; set; }

        byte[] GetImage(int quality);
        void ShowImage(byte[] image);


    }
}
