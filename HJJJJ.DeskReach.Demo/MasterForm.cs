using HJJJJ.DeskReach.Plugins.Keyboard;
using HJJJJ.DeskReach.Plugins.PluginMenu;
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
    public partial class MasterForm : Form, IPointerViewContext, IScreenViewContext, ITextMessageViewContext, IKeyboardViewContext
    {

        private int frameCount = 0;
        private DateTime lastFrameTime;
        private System.Windows.Forms.Label fpsLabel;
        private PictureBox pictureBox;
        private Client client;
        private PointerPlugin pointer;
        private ScreenPlugin screen;
        private TextMessagePlugin textMessage;
        private KeyboardPlugin keyboard;
        private DrawingBoardPlugin drawingBoard;

        public Rectangle Area { get; set; }

        public MasterForm(Client _client)
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            //初始化客户端和插件
            this.client = _client;
            pointer = new PointerPlugin(this);
            screen = new ScreenPlugin(this);
            textMessage = new TextMessagePlugin(this);
            keyboard = new KeyboardPlugin(this);
            drawingBoard = new DrawingBoardPlugin(new DrawForm());
            client.RegPlugin(pointer);
            client.RegPlugin(screen);
            client.RegPlugin(textMessage);
            client.RegPlugin(keyboard);
            client.RegPlugin(drawingBoard);

            Area = Screen.GetBounds(this);
            //监听键盘
            this.KeyPress += MainForm_KeyPress;

            //初始化界面
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
            screenPanel.Controls.Add(fpsLabel);
            screenPanel.Controls.SetChildIndex(fpsLabel, 0);

            //视频显示框
            pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;

            ///界面的鼠标控制事件
            pictureBox.MouseClick += (object sender, MouseEventArgs e) => { if (!client.IsDrawing) pointer.Action(new PointerPacket(new Point() { X = e.X * 100 / pictureBox.Width, Y = e.Y * 100 / pictureBox.Height }, PointerActionType.Left)); };
            pictureBox.MouseDoubleClick += (object sender, MouseEventArgs e) => { if (!client.IsDrawing) pointer.Action(new PointerPacket(new Point() { X = e.X * 100 / pictureBox.Width, Y = e.Y * 100 / pictureBox.Height }, PointerActionType.DoubleLeft)); };
            pictureBox.MouseWheel += (object sender, MouseEventArgs e) => { if (!client.IsDrawing) pointer.Action(new PointerPacket(new Point() { X = e.X * 100 / pictureBox.Width, Y = e.Y * 100 / pictureBox.Height }, PointerActionType.MouseWheel, e.Delta)); };
            screenPanel.Controls.Add(pictureBox);

            //picbox加入画板功能
            pictureBox.MouseDown += new MouseEventHandler(TransparentPanel_MouseDown);
            pictureBox.MouseUp += new MouseEventHandler(TransparentPanel_MouseUp);
            pictureBox.MouseMove += new MouseEventHandler(OnMouseMove);

            //board插件
            var drawingBoardToolMenu = drawingBoard.GetMenuItems(MenuPosition.ToolMenu);
            foreach (var item in drawingBoardToolMenu)
            {
                if (item is ButtonMenuItem)
                {
                    var btn = (ButtonMenuItem)item;
                    var toolBtn = new ToolStripButton();
                    toolBtn.Text = btn.Name;
                    toolBtn.Click += (object sender, EventArgs e) => btn.OnClick?.Invoke();
                    if (btn.Name == "画笔颜色") 
                    {
                        toolBtn.Click += (object sender, EventArgs e) =>
                        {
                            this.colorDialog.ShowDialog();
                            this.LineSegmentColor = colorDialog.Color;
                        };
                    }
                    this.toolStrip.Items.Add(toolBtn);
                }
                else if (item is ComboBoxMenuItem) 
                {
                    var comboBoxItem = item as ComboBoxMenuItem;
                    var comboBox = new ToolStripComboBox();
                    comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                    comboBox.Items.AddRange(comboBoxItem.Items);
                    comboBox.SelectedIndexChanged += (object sender, EventArgs e) =>
                    {
                        ToolStripComboBox text = (ToolStripComboBox)sender;
                        comboBoxItem.OnIndexChanged?.Invoke(text.SelectedItem.ToString());
                    }; ;
                    ToolStripLabel label = new ToolStripLabel();
                    label.Text = comboBoxItem.Name;
                    this.toolStrip.Items.Add(label);
                    this.toolStrip.Items.Add(comboBox);
                }
            }
            //screen插件
            var screenToolMenu = screen.GetMenuItems(MenuPosition.ToolMenu).First();
            if (screenToolMenu is ComboBoxMenuItem)
            {
                //修改视频质量combobox
                var comboBoxItem = screenToolMenu as ComboBoxMenuItem;
                var comboBox = new ToolStripComboBox();
                comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox.Items.AddRange(comboBoxItem.Items);
                comboBox.SelectedIndexChanged += (object sender, EventArgs e) =>
                {
                    ToolStripComboBox item = (ToolStripComboBox)sender;
                    comboBoxItem.OnIndexChanged?.Invoke(item.SelectedItem.ToString());
                }; ;
                ToolStripLabel label = new ToolStripLabel();
                label.Text = comboBoxItem.Name;
                this.toolStrip.Items.Add(label);
                this.toolStrip.Items.Add(comboBox);
            }
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
            keyboard.Action(new KeyboardPacket(KeyboardActionType.Single, e.KeyChar.ToString()));
        }

        private void MasterForm_Load(object sender, EventArgs e)
        {
            screen.StartReceive();         //启动图像接受
            //请求传输图像
            screen.Action(new ScreenPacket(ScreenActionType.RequestImage));

            // 创建定时器
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 50; // 设置定时器间隔（毫秒）
            timer.Tick += Timer_Tick; // 关联定时器的Tick事件处理程序
        }

        private System.Windows.Forms.Timer timer;
        System.Windows.Forms.Label messageLabel;


        private void toolStripButton5_Click(object sender, EventArgs e)
        {

            textMessage.Action(new TextMessagePacket("我是奥特曼", TextMessageActionType.SendMessage));
        }

        public void ShowMessage(string message)
        {
            var y = screenPanel.Height / 2;
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
            drawingBoard.Action(new DrawingBoardPacket(DrawingBoardActionType.OpenDrawingBoard));
        }

        private void ClearDrawingBoard_Click(object sender, EventArgs e)
        {
            mouseTrack.Clear();
            drawingBoard.Drawing(mouseTrack);
        }

        private void BrushColor_Btn_Click(object sender, EventArgs e)
        {
            colorDialog.ShowDialog();
            this.LineSegmentColor = colorDialog.Color;
        }

        private void CloseDrawingBoard_Click(object sender, EventArgs e)
        {
            IsDrawing = false;
            drawingBoard.Action(new DrawingBoardPacket(DrawingBoardActionType.CloseDrawingBoard));
            ClearDrawingBoard_Click(null, null);
        }

      
        //private void BrushSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    switch (this.BrushSizeComboBox.SelectedItem.ToString())
        //    {
        //        case "小号":
        //            this.LineSegmentWidth = 2;
        //            break;
        //        case "中号":
        //            this.LineSegmentWidth = 5;
        //            break;
        //        case "大号":
        //            this.LineSegmentWidth = 10;
        //            break;
        //        case "超大号":
        //            this.LineSegmentWidth = 15;
        //            break;
        //        default:
        //            this.LineSegmentWidth = 2;
        //            break;

        //    }
        //}
    }

}
