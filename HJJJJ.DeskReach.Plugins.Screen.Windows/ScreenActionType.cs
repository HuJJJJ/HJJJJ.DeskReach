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
        UpdateImage,

        /// <summary>
        /// 请求图像
        /// </summary>
        RequestImageRequest,

        ///响应请求图像
        RequestImageResponse,

        /// <summary>
        /// 图片确认帧
        /// </summary>
        ACK,

    }
}
