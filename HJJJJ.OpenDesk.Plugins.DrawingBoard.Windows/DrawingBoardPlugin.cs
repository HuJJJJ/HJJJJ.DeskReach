using HJJJJ.DeskReach.Plugins;
using HJJJJ.DeskReach.Plugins.Screen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows
{
    internal class DrawingBoardPlugin:BasePlugin
    {
        private IDrawingBoardViewContext ViewContext { get; set; }

        public DrawingBoardPlugin(IDrawingBoardViewContext viewContext)
        {
            ViewContext = viewContext;
        }
    }
}
