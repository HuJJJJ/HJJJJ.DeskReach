using HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows
{
    public interface IDrawingBoardViewContext
    {

        /// <summary>
        /// 初始化画板
        /// </summary>
        void InitDrawingBoard();

        /// 打开画板
        /// </summary>
        void OpenDrawingBoard();

        /// <summary>
        /// 关闭画板
        /// </summary>
        void CloseDrawingBoard();

        /// <summary>
        /// 清空画板
        /// </summary>
        void ClearDrawingBoard();

        /// <summary>
        /// 画画
        /// </summary>
        /// <param name="lines"></param>
        void Drawing(List<LineSegment> lines);


    }
}
