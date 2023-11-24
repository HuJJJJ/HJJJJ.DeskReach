using HJJJJ.DeskReach.Plugins;
using HJJJJ.DeskReach.Plugins.PluginMenu;
using HJJJJ.DeskReach.Plugins.Screen;
using HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        private int LineSegmentWidth;
        private Color LineSegmentColor;

        public override List<MenuItem> GetMenuItems(MenuPosition pos)
        {
            var menu = new List<MenuItem>();
            switch (pos)
            {
                case MenuPosition.TopMenu:
                    break;
                case MenuPosition.Screen:
                    break;
                case MenuPosition.ToolBar:
                    break;
                case MenuPosition.ToolMenu:
                    menu.AddRange(new MenuItem[]
                    {
                        new ButtonMenuItem
                        {
                        Name="画笔",
                        OnClick =OpenBoard
                        },
                        new ButtonMenuItem
                        {
                        Name ="画笔颜色",
                        
                        },
                        new ComboBoxMenuItem
                        {
                        Name ="画笔大小",
                        Items = new object[]{"小","中","大" }
                        },
                            new ButtonMenuItem
                        {
                        Name="清空画板",
                        OnClick = ClearBoard
                        },
                        new ButtonMenuItem
                        {
                        Name = "关闭画板",
                        OnClick = CloseBoard
                        },
                    
                    }

                    );
                    break;
                case MenuPosition.StatusMenu:
                    break;
            }
            return menu;
        }

        /// <summary>
        /// 打开画板
        /// </summary>
        private void OpenBoard(object sender,EventArgs e)
        {
            //修改当前为画画模式
            client.IsDrawing = true;
            Action(new DrawingBoardPacket(DrawingBoardActionType.OpenDrawingBoard));
        }

        /// <summary>
        /// 关闭画板
        /// </summary>
        private void CloseBoard(object sender, EventArgs e)
        {
            //修改当前为画画模式
            client.IsDrawing = true;
            Action(new DrawingBoardPacket(DrawingBoardActionType.CloseDrawingBoard));
        }

        /// <summary>
        /// 清除画板
        /// </summary>
        private void ClearBoard(object sender, EventArgs e)
        {

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
