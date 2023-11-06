using System;
using System.Collections.Generic;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Pointer
{
    public enum PointerActionType:byte
    {
        /// <summary>
        /// 鼠标左键
        /// </summary>
        Left,

        /// <summary>
        /// 鼠标左键双击
        /// </summary>
        DoubleLeft,

        /// <summary>
        /// 鼠标右键
        /// </summary>
        Right,


        /// <summary>
        /// 鼠标右键双击
        /// </summary>
        DoubleRight,

        /// <summary>
        /// 鼠标中键
        /// </summary>
        Middle,

        /// <summary>
        /// 鼠标移动
        /// </summary>
        Move,

        /// <summary>
        /// 鼠标滚动
        /// </summary>
        MouseWheel,
    }
}
