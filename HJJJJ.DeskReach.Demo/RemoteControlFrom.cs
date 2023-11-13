using HJJJJ.DeskReach.Plugins.Keyboard;
using HJJJJ.DeskReach.Plugins.Pointer;
using HJJJJ.DeskReach.Plugins.Screen;
using HJJJJ.DeskReach.Plugins.TextMessage.Windows;
using HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows;
using HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace HJJJJ.DeskReach.Demo
{
    public partial class RemoteControlFrom : Form, IPointerViewContext, IScreenViewContext, ITextMessageViewContext, IKeyboardViewContext
    {

        private int frameCount = 0;
        private DateTime lastFrameTime;
        private System.Windows.Forms.Label fpsLabel;
        private PictureBox pictureBox;


        public Rectangle Area { get; set; }

        public RemoteControlFrom()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Area = Screen.GetBounds(this);
            //监听键盘
            this.KeyPress += MainForm_KeyPress;
            InitView();
        }


        /// <summary>
        /// 初始化界面
        /// </summary>
        public void InitView()
        {
            //帧率显示
            fpsLabel = new System.Windows.Forms.Label();
            fpsLabel.AutoSize = true;
            panel2.Controls.Add(fpsLabel);
            panel2.Controls.SetChildIndex(fpsLabel, 0);

            //视频质量选择框
            comboBox1.Items.AddRange(new object[]
            { imageQuality.Speed,
              imageQuality.Low,
              imageQuality.Nomal,
              imageQuality.High,
              imageQuality.Quality
            });
            comboBox1.SelectedIndex = 1;

            //设置画板字体
            BrushSizeComboBox.Items.AddRange(new object[] { "小号", "中号", "大号", "超大号" });
            BrushSizeComboBox.SelectedIndex = 0;

            //视频显示框
            pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;

            ///界面的鼠标控制事件
            pictureBox.MouseClick += (object sender, MouseEventArgs e) => { if (!IsDrawing) Store.pointer.Action(new PointerPacket(new Point() { X = e.X * 100 / pictureBox.Width, Y = e.Y * 100 / pictureBox.Height }, PointerActionType.Left)); };
            pictureBox.MouseDoubleClick += (object sender, MouseEventArgs e) => { if (!IsDrawing) Store.pointer.Action(new PointerPacket(new Point() { X = e.X * 100 / pictureBox.Width, Y = e.Y * 100 / pictureBox.Height }, PointerActionType.DoubleLeft)); };
            pictureBox.MouseWheel += (object sender, MouseEventArgs e) => { if (!IsDrawing) Store.pointer.Action(new PointerPacket(new Point() { X = e.X * 100 / pictureBox.Width, Y = e.Y * 100 / pictureBox.Height }, PointerActionType.MouseWheel, e.Delta)); };
            panel2.Controls.Add(pictureBox);

            //picbox加入画板功能
            pictureBox.MouseDown += new MouseEventHandler(TransparentPanel_MouseDown);
            pictureBox.MouseUp += new MouseEventHandler(TransparentPanel_MouseUp);
            pictureBox.MouseMove += new MouseEventHandler(OnMouseMove);
        }


        /// <summary>
        /// 获取当前屏幕的截图
        /// </summary>
        /// <param name="quality"></param>
        /// <returns></returns>
        public byte[] GetImage(int quality)
        {
            var sendFrame = WindowsAPIScreenCapture.CaptureScreen(quality);
            //frameCount++;
            //if (DateTime.Now - lastFrameTime >= TimeSpan.FromSeconds(1))
            //{
            //    // 显示帧率并重置帧数和计时器
            //    fpsLabel.Text = $"FPS: {frameCount}";
            //    frameCount = 0;
            //    lastFrameTime = DateTime.Now;
            //}
            return sendFrame;
        }

        /// <summary>
        /// 显示回传的图片
        /// </summary>
        /// <param name="image"></param>
        public void ShowImage(byte[] image)
        {
            var tempFrame = image.BytesToBitmap();
            int targetWidth = this.pictureBox.Width;
            int targetHeight = (int)((float)tempFrame.Height / tempFrame.Width * targetWidth);

            //var frame = tempFrame.GetThumbnailImage(targetWidth, targetHeight, null, IntPtr.Zero);
            //tempFrame.Dispose();
            Bitmap resizedBitmap = new Bitmap(tempFrame, targetWidth, targetHeight);
            tempFrame.Dispose();
            this.pictureBox.Image = resizedBitmap;
            // 检查是否达到了一秒钟
            if (DateTime.Now - lastFrameTime >= TimeSpan.FromSeconds(1))
            {
                // 显示帧率并重置帧数和计时器
                fpsLabel.Text = $"FPS: {frameCount}";
                frameCount = 0;
                lastFrameTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 监听键盘事件并发送给受控端
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            Store.keyboard.Action(new KeyboardPacket(KeyboardActionType.Single, e.KeyChar.ToString()));
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Store.screen == null) return;
            int quality = 0;
            switch ((imageQuality)comboBox1.SelectedIndex)
            {
                case imageQuality.Speed:
                    quality = 0;
                    break;
                case imageQuality.Low:
                    quality = 20;
                    break;
                case imageQuality.Nomal:
                    quality = 50;
                    break;
                case imageQuality.High:
                    quality = 80;
                    break;
                case imageQuality.Quality:
                    quality = 100;
                    break;
            }
            Store.screen.Action(new ScreenPacket(ScreenActionType.ImageQuality, null, quality));
        }

        private void RemoteControlFrom_Load(object sender, EventArgs e)
        {
            Store.screen.StartReceive();         //启动图像接受
            //请求传输图像
            Store.screen.Action(new ScreenPacket(ScreenActionType.RequestImage));

            // 创建定时器
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 50; // 设置定时器间隔（毫秒）
            timer.Tick += Timer_Tick; // 关联定时器的Tick事件处理程序
        }
        private System.Windows.Forms.Timer timer;
        System.Windows.Forms.Label messageLabel;


        private void toolStripButton5_Click(object sender, EventArgs e)
        {

            Store.textMessage.Action(new TextMessagePacket("我是奥特曼", TextMessageActionType.SendMessage));
        }

        public void ShowMessage(string message)
        {
            var y = panel2.Height / 2;
            this.Invoke(new MethodInvoker(delegate
            {
                var lastFrameTime = DateTime.Now;
                messageLabel = new System.Windows.Forms.Label();
                messageLabel.Text = message;
                messageLabel.Location = new Point(0, y);
                messageLabel.Font = new Font("微软雅黑", 12f); // 设置字体为微软雅黑，大小为12
                messageLabel.ForeColor = Color.White;
                messageLabel.BackColor = Color.Transparent;
                messageLabel.Parent = pictureBox;
                pictureBox.BackColor = Color.Transparent;
                messageLabel.AutoSize = true;
                pictureBox.Controls.Add(messageLabel);
                pictureBox.Controls.SetChildIndex(messageLabel, 0);
                timer.Start();
            }));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (messageLabel.Visible == false)
            {
                messageLabel.Visible = true; // 当定时器开始运行时，让标签显示
            }
            else
            {
                if (messageLabel.ForeColor.A > 0)
                {
                    messageLabel.ForeColor = Color.FromArgb(messageLabel.ForeColor.A - 5, messageLabel.ForeColor);
                }
                else
                {
                    timer.Stop(); // 当标签完全消失时，停止定时器
                    messageLabel.Visible = false;
                }
            }
        }

        /// <summary>
        /// 模拟按键输入
        /// </summary>
        /// <param name="keyCahr"></param>
        public void SendKey(string keyCahr) => SendKeys.SendWait(keyCahr);

        private void BrushStatus_Btn_Click(object sender, EventArgs e)
        {
            IsDrawing = true;
            Store.drawingBoard.Action(new DrawingBoardPacket(DrawingBoardActionType.OpenDrawingBoard));
        }

        private void ClearDrawingBoard_Click(object sender, EventArgs e)
        {
            mouseTrack.Clear();
            Store.drawingBoard.Drawing(mouseTrack);
        }

        private void BrushColor_Btn_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            this.LineSegmentColor = colorDialog1.Color;
        }

        private void CloseDrawingBoard_Click(object sender, EventArgs e)
        {
            IsDrawing = false;
            Store.drawingBoard.Action(new DrawingBoardPacket(DrawingBoardActionType.CloseDrawingBoard));
            ClearDrawingBoard_Click(null,null);
        }

        private void BrushSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.BrushSizeComboBox.SelectedItem.ToString())
            {
                case "小号":
                    this.LineSegmentWidth = 2;
                    break;
                case "中号":
                    this.LineSegmentWidth = 5;
                    break;
                case "大号":
                    this.LineSegmentWidth = 10;
                    break;
                case "超大号":
                    this.LineSegmentWidth = 15;
                    break;
                default:
                    this.LineSegmentWidth = 2;
                    break;

            }
        }
    }



    public enum imageQuality : int
    {
        Speed,
        Low,
        Nomal,
        High,
        Quality
    }
}
