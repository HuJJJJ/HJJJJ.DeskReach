using System;
using System.Collections.Generic;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Screen.Windows
{

    /// <summary>
    /// 屏幕具体操作
    /// </summary>
    public enum ScreenActionType
    {
        /// <summary>
        /// 图像帧
        /// </summary>
        ImageFrame,

        /// <summary>
        /// 请求图像
        /// </summary>
        RequestImage,

        /// <summary>
        /// 修改图片质量
        /// </summary>
        ImageQuality,

        /// <summary>
        /// 图片确认帧
        /// </summary>
        ACK,
    }
}
