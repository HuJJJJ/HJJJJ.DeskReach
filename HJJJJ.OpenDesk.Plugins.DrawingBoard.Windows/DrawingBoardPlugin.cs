using HJJJJ.DeskReach.Plugins;
using HJJJJ.DeskReach.Plugins.Screen;
using HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows
{
    public class DrawingBoardPlugin : BasePlugin
    {
        private IDrawingBoardViewContext ViewContext { get; set; }
        private new event EventHandler<byte[]> OnDataReceived;
        private AutoResetEvent AutoResetEvent = new AutoResetEvent(false);
        public DrawingBoardPlugin(IDrawingBoardViewContext viewContext)
        {
            ViewContext = viewContext;
            OnDataReceived += DrawingBoardPlugin_OnDataReceived;
            viewContext.InitDrawingBoard();
        }


        /// <summary>
        /// 接收消息处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void RaiseDataReceived(object sender, byte[] e) => SafelyInvokeCallback(() => { OnDataReceived?.Invoke(sender, e); });
        private void DrawingBoardPlugin_OnDataReceived(object sender, byte[] e)
        {
            var data = new DrawingBoardPacket(e);
            switch (data.Code)
            {
                case DrawingBoardActionType.OpenDrawingBoard:
                    ViewContext.OpenDrawingBoard();
                    break;
                case DrawingBoardActionType.CloseDrawingBoard:
                    ViewContext.CloseDrawingBoard();
                    break;
                case DrawingBoardActionType.DrawingBoardStroke:
                    Action(new DrawingBoardPacket(DrawingBoardActionType.ACK));
                    ViewContext.Drawing(data.Lines);
                    break;
                case DrawingBoardActionType.ClearDrawingBoard:
                    ViewContext.ClearDrawingBoard();
                    break;
                case DrawingBoardActionType.ACK:
                    AutoResetEvent.Set();
                    break;
            }
        }
        public void Drawing(List<LineSegment> mouseTrack)
        {
            Action(new DrawingBoardPacket(DrawingBoardActionType.DrawingBoardStroke, mouseTrack));
            AutoResetEvent.WaitOne();
        }

        /// <summary>
        /// 操作鼠标
        /// </summary>
        public void Action(DrawingBoardPacket packet) => Send(packet);


    }
}
