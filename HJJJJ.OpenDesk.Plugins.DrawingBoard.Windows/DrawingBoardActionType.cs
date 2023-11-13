using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows
{
    public enum DrawingBoardActionType
    {
        /// <summary>
        /// 打开画板
        /// </summary>
        OpenDrawingBoard,

        /// <summary>
        /// 关闭画板
        /// </summary>
        CloseDrawingBoard,

        /// <summary>
        /// 传送画板轨迹
        /// </summary>
        DrawingBoardStroke,

        /// <summary>
        /// 清空画板
        /// </summary>
        ClearDrawingBoard,

        /// <summary>
        /// 确认帧
        /// </summary>
        ACK,
    }
}
