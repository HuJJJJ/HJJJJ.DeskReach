using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows
{
    internal interface IDrawingBoardViewContext
    {
         /// <summary>
        /// 当前屏幕分辨率
        /// </summary>
        Rectangle Bounds { get; set; }
        void Drawing();
    }
}
