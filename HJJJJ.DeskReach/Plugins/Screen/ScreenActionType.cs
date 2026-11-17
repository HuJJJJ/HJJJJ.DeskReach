using System;
using System.Collections.Generic;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Screen
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

        ///修改图片质量
        ImageQuality,

        /// <summary>
        /// 图片确认帧
        /// </summary>
        ACK,

        /// <summary>
        /// 禁用
        /// </summary>
        Disable,

        /// <summary>
        /// 启用
        /// </summary>
        Enable,

        /// <summary>差量基准缺失或接收失败时请求完整关键帧。</summary>
        RequestKeyFrame


    }
}
