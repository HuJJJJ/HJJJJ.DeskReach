using HJJJJ.DeskReach.Plugins.Keyboard;
using HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows
{
    public partial class DrawForm : Form, IDrawingBoardViewContext
    {

        public Graphics gs;
        Pen pen;
        Rectangle Area;
        public DrawForm()
        {
            InitializeComponent();
            this.TransparencyKey = BackColor;
            this.pen = new Pen(Color.Black, 3f);//画笔
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;//本窗体最大化
            this.TopMost = true;
            this.gs = CreateGraphics();//创建窗体画板
            // 订阅重绘事件
            // this.FormBorderStyle = FormBorderStyle.None;
        }
        private void DrawForm_Paint(object sender, PaintEventArgs e)
        {
            // 清空画板内容
            e.Graphics.Clear(BackColor);
        }

        public void Drawing(List<LineSegment> strokes)
        {
            Area = Screen.GetBounds(this);
            for (int i = 0; i < strokes.Count; i++)
            {
                for (int j = 0; j < strokes[i].Tracks.Count; j++)
                {
                    strokes[i].Tracks[j] = new Point()
                    {
                        X = Convert.ToInt32((double)(strokes[i].Tracks[j].X / 100.0) * Area.Width),
                        Y = (Convert.ToInt32((double)(strokes[i].Tracks[j].Y / 100.0) * Area.Height)),
                    };
                }
            }
            using ( pen = new Pen(Color.Black, 3f))
            {
                foreach (LineSegment line in strokes)
                {
                    pen.Color = line.Color;
                    pen.Width = line.Width;
                    if (line.Tracks.Count > 1)
                    {
                        for (int i = 0; i < line.Tracks.Count - 1; i++)
                        {
                            gs.DrawLine(pen, line.Tracks[i], line.Tracks[i + 1]);  // 绘制每一段线条
                        }
                    }
                }
            }
          
        }

        

        public void OpenDrawingBoard()
        {
            this.Show();
        }

        public void CloseDrawingBoard()
        {
            this.Hide();
        }

        public void ClearDrawingBoard()
        {

        }

        public void InitDrawingBoard()
        {
        }

        private void DrawForm_Load(object sender, EventArgs e)
        {
   

        }
    }
}
