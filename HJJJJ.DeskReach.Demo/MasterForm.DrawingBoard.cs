using HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HJJJJ.DeskReach.Demo
{
    public partial class MasterForm
    {
        /// <summary>
        /// 是否需要重绘
        /// </summary>
        private bool isInvalidated = false; //重绘请求

        /// <summary>
        /// 线段颜色
        /// </summary>
        public Color LineSegmentColor;

        public int LineSegmentWidth;


        object sendClock = new object();


        /// <summary>
        /// 当前是否是绘画状态
        /// </summary>
        public bool IsDrawing { get; set; }


        private List<LineSegment> mouseTrack = new List<LineSegment>();  // 鼠标轨迹集合

        public void InitDrawingBoard()
        {
            DoubleBuffered = true;
            LineSegmentColor = Color.Red;
            LineSegmentWidth = 2;
        }


        /// <summary>
        /// 清空画板
        /// </summary>
        public void ClearMouseTrack()
        {
            mouseTrack.Clear();  // 清空鼠标轨迹集合
        }

        /// <summary>
        /// 当鼠标按下，表示开启一段新的线条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void TransparentPanel_MouseDown(object sender, MouseEventArgs e)
        {
            mouseTrack.Add(new LineSegment()
            {
                Color = LineSegmentColor,
                Width = LineSegmentWidth,
            });
        }

        /// <summary>
        /// 鼠标滑动时，记录坐标并重绘
        /// </summary>
        /// <param name="e"></param>
        protected void OnMouseMove(object sender, MouseEventArgs e)
        {
            PointerViewBounds = Screen.GetBounds(this);
            base.OnMouseMove(e);
            if (client.IsDrawing && Control.MouseButtons == MouseButtons.Left)
            {
                Point currentPoint = new Point()
                {
                    X = e.Location.X * 100 / pictureBox.Width,
                    Y = e.Location.Y * 100 / pictureBox.Height
                };
                mouseTrack[mouseTrack.Count - 1].Tracks.Add(currentPoint);  // 添加当前鼠标位置到鼠标轨迹集合
                isInvalidated = true;
                SendStroke();
            }
        }

        public void SendStroke()
        {
            lock (sendClock)
            {
                if (!isInvalidated) return;
                for (int i = 0; i < mouseTrack.Count; i++)
                {
                    mouseTrack[i].Tracks = mouseTrack[i].Tracks.GroupBy(item => (item.X, item.Y)).Select(item => item.First()).ToList();
                }
                drawingBoard.Drawing(mouseTrack);
            }
            //lock (clearClock)
            //{
            //    mouseTrack.Clear();  // 清空鼠标轨迹集合
            //}
        }

        /// <summary>
        /// 鼠标抬起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TransparentPanel_MouseUp(object sender, MouseEventArgs e)
        {
            //SendStroke();  // 触发重绘
        }
    }
}
