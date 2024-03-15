using System;
using System.Collections.Generic;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Pointer
{
    public enum PointerActionType:byte
    {


        /// <summary>
        /// 左键按下
        /// </summary>
        LeftDown,

        /// <summary>
        /// 左键键抬起
        /// </summary>
        LeftUp,

        /// <summary>
        /// 鼠标左键双击
        /// </summary>
        DoubleLeftClick,



        /// <summary>
        /// 鼠标按下
        /// </summary>
        RightDown,

        /// <summary>
        /// 鼠标抬起
        /// </summary>
        RightUp,

        /// <summary>
        /// 鼠标右键双击
        /// </summary>
        DoubleRightClick,



        /// <summary>
        /// 鼠标中键按下
        /// </summary>
        MiddleDown,

        /// <summary>
        /// 鼠标中键抬起
        /// </summary>
        MiddleUp,

        /// <summary>
        /// 鼠标滚动
        /// </summary>
        MiddleWheel,

        /// <summary>
        /// 鼠标中键双击
        /// </summary>
        DoubleMiddleClick,



        /// <summary>
        /// 鼠标移动
        /// </summary>
        Move,



        /// <summary>
        /// 鼠标侧键1按下
        /// </summary>
        XButton1Down,

        /// <summary>
        /// 鼠标侧键1抬起
        /// </summary>
        XButton1Up,

        /// <summary>
        /// 鼠标侧键1双击
        /// </summary>
        DoubleXButton1Click,



        /// <summary>
        /// 鼠标侧键2按下
        /// </summary>
        XButton2Down,

        /// <summary>
        /// 鼠标侧键2抬起
        /// </summary>
        XButton2Up,

        /// <summary>
        /// 鼠标侧键2双击
        /// </summary>
        DoubleXButton2Click,


        /// <summary>
        /// 空
        /// </summary>
        None,

        /// <summary>
        /// 禁用
        /// </summary>
        Disable,

        /// <summary>
        /// 启用
        /// </summary>
        Enable
    }
}
