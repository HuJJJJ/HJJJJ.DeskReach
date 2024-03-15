using HJJJJ.DeskReach.Plugins.CommandPrompt.Windows;
using HJJJJ.DeskReach.Plugins.FileTransfer.Windows;
using HJJJJ.DeskReach.Plugins.Keyboard;
using HJJJJ.DeskReach.Plugins.PluginMenu;
using HJJJJ.DeskReach.Plugins.Pointer;
using HJJJJ.DeskReach.Plugins.Screen;
using HJJJJ.DeskReach.Plugins.Screen.Windows.Package;
using HJJJJ.DeskReach.Plugins.TextMessage.Windows;
using HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static HJJJJ.DeskReach.Windows.MainFrom;

namespace HJJJJ.DeskReach.Windows
{
    public partial class MasterForm : Form, IPointerViewContext, IScreenViewContext, ITextMessageViewContext, IKeyboardViewContext, IFileTransferViewContext
    {

        private int frameCount = 0;
        private DateTime lastFrameTime;
        private Client client;
        private PointerPlugin pointer;
        private ScreenPlugin screen;
        private TextMessagePlugin textMessage;
        private KeyboardPlugin keyboard;
        private DrawingBoardPlugin drawingBoard;
        private CommandPromptPlugin command;
        private FileTransferPlugin fileTransfer;
        private ConnectionType ConnectionMode;

        public Rectangle PointerViewBounds { get; set; }
        public Rectangle ScreenViewBounds { get; set; }
        public MasterForm(Client _client, ConnectionType type = ConnectionType.Control)
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            ConnectionMode = type;
            //获取缩放比例
            //获取分辨率
            var bounds = Screen.GetBounds(this);
            PointerViewBounds = bounds;
            ScreenViewBounds = bounds;

            //初始化客户端和插件
            this.client = _client;

            //根据控制模式按需初始化插件
            fileTransfer = new FileTransferPlugin(this);
            screen = new ScreenPlugin(this);
            pointer = new PointerPlugin(this);
            textMessage = new TextMessagePlugin(this);
            keyboard = new KeyboardPlugin(this);
            drawingBoard = new DrawingBoardPlugin(new DrawForm(this));
            command = new CommandPromptPlugin(new CMDForm(client, this));
        
            switch (ConnectionMode)
            {
                case ConnectionType.Control:
                    client.RegPlugin(fileTransfer);
                    client.RegPlugin(textMessage);
                    client.RegPlugin(keyboard);
                    client.RegPlugin(drawingBoard);
                    client.RegPlugin(command);
                    client.RegPlugin(pointer);
                    client.RegPlugin(screen);
                    break;
                case ConnectionType.View:
                    client.RegPlugin(screen);
                    break;
            }
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
            //视频显示框
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox.AllowDrop = true;
            pictureBox.BringToFront();

            //界面的鼠标控制事件
            MouseEventHandler mouseMove = (object sender, MouseEventArgs e) => pointer.Action(new PointerPacket(new Point() { X = e.X * 100 / pictureBox.Width, Y = e.Y * 100 / pictureBox.Height }, PointerActionType.Move));
            pictureBox.MouseDown += (object sender, MouseEventArgs e) =>
            {
                if (!client.IsDrawing)
                {
                    PointerActionType type = PointerActionType.None;
                    switch (e.Button)
                    {
                        case MouseButtons.Left:
                            type = PointerActionType.LeftDown;
                            pictureBox.MouseMove += mouseMove;
                            break;
                        case MouseButtons.None:
                            type = PointerActionType.None;
                            break;
                        case MouseButtons.Right:
                            type = PointerActionType.RightDown;
                            break;
                        case MouseButtons.Middle:
                            type = PointerActionType.MiddleDown;
                            break;
                        case MouseButtons.XButton1:
                            type = PointerActionType.XButton1Down;
                            break;
                        case MouseButtons.XButton2:
                            type = PointerActionType.XButton2Down;
                            break;
                        default:
                            type = PointerActionType.None;
                            break;
                    }
                    pointer.Action(new PointerPacket(new Point() { X = e.X * 100 / pictureBox.Width, Y = e.Y * 100 / pictureBox.Height }, type));
                }
            }; ;
            pictureBox.MouseUp += (object sender, MouseEventArgs e) =>
            {
                if (!client.IsDrawing)
                {
                    PointerActionType type = PointerActionType.None;
                    switch (e.Button)
                    {
                        case MouseButtons.Left:
                            type = PointerActionType.LeftUp;
                            pictureBox.MouseMove -= mouseMove;
                            break;
                        case MouseButtons.None:
                            type = PointerActionType.None;
                            break;
                        case MouseButtons.Right:
                            type = PointerActionType.RightUp;
                            break;
                        case MouseButtons.Middle:
                            type = PointerActionType.MiddleUp;
                            break;
                        case MouseButtons.XButton1:
                            type = PointerActionType.XButton1Up;
                            break;
                        case MouseButtons.XButton2:
                            type = PointerActionType.XButton2Up;
                            break;
                        default:
                            type = PointerActionType.None;
                            break;
                    }
                    pointer.Action(new PointerPacket(new Point() { X = e.X * 100 / pictureBox.Width, Y = e.Y * 100 / pictureBox.Height }, type));
                }
            }; ;
            pictureBox.MouseDoubleClick += (object sender, MouseEventArgs e) =>
            {
                if (!client.IsDrawing)
                {
                    PointerActionType type = PointerActionType.None;
                    switch (e.Button)
                    {
                        case MouseButtons.Left:
                            type = PointerActionType.DoubleLeftClick;
                            break;
                        case MouseButtons.Right:
                            type = PointerActionType.DoubleRightClick;
                            break;
                        case MouseButtons.Middle:
                            type = PointerActionType.DoubleMiddleClick;
                            break;
                        case MouseButtons.XButton1:
                            type = PointerActionType.DoubleXButton1Click;
                            break;
                        case MouseButtons.XButton2:
                            type = PointerActionType.DoubleXButton2Click;
                            break;
                        case MouseButtons.None:
                        default:
                            type = PointerActionType.None;
                            break;
                    }
                    pointer.Action(new PointerPacket(new Point() { X = e.X * 100 / pictureBox.Width, Y = e.Y * 100 / pictureBox.Height }, type));
                }

            }; ;
            pictureBox.MouseWheel += (object sender, MouseEventArgs e) => { if (!client.IsDrawing) pointer.Action(new PointerPacket(new Point() { X = e.X * 100 / pictureBox.Width, Y = e.Y * 100 / pictureBox.Height }, PointerActionType.MiddleWheel, e.Delta)); };
            //picbox加入画板功能
            pictureBox.MouseDown += new MouseEventHandler(TransparentPanel_MouseDown);
            pictureBox.MouseMove += new MouseEventHandler(OnMouseMove);
            screenPanel.Controls.Add(pictureBox);

            //picbox文件拖拽功能
            pictureBox.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
            };
            pictureBox.DragDrop += (s, e) =>
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // 对拖放的文件进行处理
                for (int i = 0; i < files.Length; i++)
                {
                    if (File.Exists(files[i]))
                    {
                        Point relativePoint = pictureBox.PointToClient(new Point(e.X, e.Y));
                        // 处理文件
                        pointer.Action(new PointerPacket(new Point() { X = relativePoint.X * 100 / pictureBox.Width, Y = relativePoint.Y * 100 / pictureBox.Height }, PointerActionType.LeftDown));
                        pointer.Action(new PointerPacket(new Point() { X = relativePoint.X * 100 / pictureBox.Width, Y = relativePoint.Y * 100 / pictureBox.Height }, PointerActionType.LeftUp));
                        fileTransfer.SendFile(files[i]);
                    }
                    else if (Directory.Exists(files[i]))
                    {
                        foreach (var dir in files)
                        {
                            var dirFiles = Directory.GetFiles(dir);
                            foreach (var file in dirFiles)
                            {
                                fileTransfer.SendFile(files[i]);
                            }
                        }
                        // 处理文件夹
                        MessageBox.Show("文件夹");
                    }
                    else
                    {
                        // 都不是
                        MessageBox.Show("狗屎");
                    }

                }
            };
            InitPluginMenu();
        }



        public void InitPluginMenu()
        {
            this.toolStrip.Items.Clear();
            this.statusStrip.Items.Clear();

            switch (ConnectionMode)
            {
                case ConnectionType.Control:
                    InitCmdPluginMenu();
                    InitDarwingBoardPluginMenu();
                    InitScreenPluginMenu();
                    break;
                case ConnectionType.View:
                    InitScreenPluginMenu();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 初始化screen插件菜单
        /// </summary>
        private void InitScreenPluginMenu()
        {
            //screen插件
            var screenToolMenu = screen.GetMenuItems(MenuPosition.ToolMenu).FirstOrDefault();
            if (screenToolMenu == null) return;
            if (screenToolMenu is ComboBoxMenuItem)
            {
                //修改视频质量combobox
                var comboBoxItem = screenToolMenu as ComboBoxMenuItem;
                var comboBox = new ToolStripComboBox();
                comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox.Items.AddRange(comboBoxItem.Items);
                comboBox.SelectedIndex = 0;
                comboBox.SelectedIndexChanged += (object sender, EventArgs e) =>
                {
                    ToolStripComboBox item = (ToolStripComboBox)sender;
                    comboBoxItem.OnIndexChanged?.Invoke(screenToolMenu, new ComboBoxSelectedEvnetArgs { Text = item.SelectedItem.ToString() });
                    this.ActiveControl = null;
                }; ;
                ToolStripLabel label = new ToolStripLabel();
                label.Text = comboBoxItem.Name;
                this.toolStrip.Items.Add(label);
                this.toolStrip.Items.Add(comboBox);
            }
        }

        /// <summary>
        /// 初始化画板插件菜单
        /// </summary>
        private void InitDarwingBoardPluginMenu()
        {
            //board插件
            var drawingBoardToolMenu = drawingBoard.GetMenuItems(MenuPosition.ToolMenu);
            if (drawingBoardToolMenu == null) return;
            foreach (var item in drawingBoardToolMenu)
            {
                if (item is ButtonMenuItem)
                {
                    var btn = (ButtonMenuItem)item;
                    var toolBtn = new ToolStripButton();
                    toolBtn.Text = btn.Name;
                    toolBtn.Click += (object sender, EventArgs e) => btn.OnClick?.Invoke(btn, e);
                    if (btn.Name == "画笔颜色")
                    {
                        toolBtn.Click += (object sender, EventArgs e) =>
                        {
                            this.colorDialog.ShowDialog();
                            this.LineSegmentColor = colorDialog.Color;
                        };
                    }

                    if (btn.Name == "清空画板")
                    {
                        toolBtn.Click += (object sender, EventArgs e) =>
                        {
                            ClearMouseTrack();
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
                        switch (text.SelectedItem.ToString())
                        {
                            case "小":
                                this.LineSegmentWidth = 2;
                                break;
                            case "中":
                                this.LineSegmentWidth = 4;
                                break;
                            case "大":
                                this.LineSegmentWidth = 6;
                                break;
                        }
                        this.ActiveControl = null;
                    };
                    ToolStripLabel label = new ToolStripLabel();
                    label.Text = comboBoxItem.Name;
                    this.toolStrip.Items.Add(label);
                    this.toolStrip.Items.Add(comboBox);
                }
            }
        }

        /// <summary>
        /// 初始化cmd插件菜单
        /// </summary>
        private void InitCmdPluginMenu()
        {
            //cmd插件
            var cmdMenu = command.GetMenuItems(MenuPosition.StatusMenu);
            foreach (var item in cmdMenu)
            {
                if (item is ButtonMenuItem)
                {
                    var btn = (ButtonMenuItem)item;
                    var toolBtn = new ToolStripButton();
                    toolBtn.Text = btn.Name;
                    toolBtn.Click += (object sender, EventArgs e) => { btn.OnClick?.Invoke(btn, e); };
                    this.statusStrip.Items.Add(toolBtn);
                }
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
            return sendFrame;
        }

        /// <summary>
        /// 显示回传的图片
        /// </summary>
        /// <param name="image"></param>
        public void ShowImage(byte[] image)
        {
            var tempFrame = image.ToBitmap();
            int targetWidth = this.pictureBox.Width;
            int targetHeight = (int)((float)tempFrame.Height / tempFrame.Width * targetWidth);
            this.pictureBox.Image = tempFrame.GetThumbnailImage(targetWidth, targetHeight, null, IntPtr.Zero);
        }

        private Bitmap bitmap;
        public void InitBitmap(int width, int height, PixelFormat format)
        {
            bitmap = new Bitmap(width, height, format);
        }
        /// <summary>
        /// 显示回传的图片
        /// </summary>
        /// <param name="image"></param>
        public void ShowImage(Bitmap newBitmap)
        {
            //var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            //var newBitmapData = newBitmap.LockBits(new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), ImageLockMode.ReadWrite, newBitmap.PixelFormat);
            //var intPtr = newBitmapData.Scan0;
            //var bytes = new byte[newBitmap.Width * newBitmap.Height * 3];
            //Marshal.Copy(intPtr, bytes, 0, bytes.Length);
            //Marshal.Copy(bytes, 0, bitmapData.Scan0, bytes.Length);
            //newBitmap.UnlockBits(newBitmapData);
            //bitmap.UnlockBits(bitmapData);
            this.pictureBox.Image = newBitmap;

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
            screen.StartReceive();//启动图像接受
                                  //  screen.Action(new RequestImageRequest());//请求传输图像
            screen.Action(new ScreenPacket(ScreenActionType.RequestImage));
        }

        public void ShowMessage(string message)
        {
            //var y = screenPanel.Height / 2;
            //this.Invoke(new MethodInvoker(delegate
            //{
            //    var lastFrameTime = DateTime.Now;
            //    messageLabel = new System.Windows.Forms.Label();
            //    messageLabel.Text = message;
            //    messageLabel.Location = new Point(0, y);
            //    messageLabel.Font = new Font("微软雅黑", 12f); // 设置字体为微软雅黑，大小为12
            //    messageLabel.ForeColor = Color.White;
            //    messageLabel.BackColor = Color.Transparent;
            //    messageLabel.Parent = pictureBox;
            //    pictureBox.BackColor = Color.Transparent;
            //    messageLabel.AutoSize = true;
            //    pictureBox.Controls.Add(messageLabel);
            //    pictureBox.Controls.SetChildIndex(messageLabel, 0);
            //    timer.Start();
            //}));
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



        private void screenPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        public void UpdateDirectories(TreeNode node)
        {
            throw new NotImplementedException();
        }

        public void UpdatetLogicalDrives(List<TreeNode> nodes)
        {
            throw new NotImplementedException();
        }
    }

    public static class MenuItemExtensions
    {
        public static ToolStripItem ToToolStripItem(this Plugins.PluginMenu.MenuItem item)
        {
            switch (item.ItemType)
            {
                case MenuItemType.Button:
                    {
                        var buttonItem = item as ButtonMenuItem;
                        return new ToolStripMenuItem()
                        {
                            Text = item.Name,
                            Enabled = item.Enable
                        }.With(tsmi => tsmi.Click += (s, e) => buttonItem?.OnClick(buttonItem, e));
                    }
                case MenuItemType.Input:
                    break;
                case MenuItemType.Drop:
                    var drop = item as DropMenuItem;
                    return new ToolStripMenuItem()
                    {
                        Text = item.Name,
                        Enabled = item.Enable
                    }.With(x => x.DropDownItems.AddRange(drop.DropItems.Select(i => i.ToToolStripItem()).ToArray()));

                case MenuItemType.ComboBox:
                    {
                        var comboBoxItem = item as ComboBoxMenuItem;
                        return new ToolStripComboBox() { Enabled = item.Enable }.With(cb =>
                        {
                            cb.Items.AddRange(comboBoxItem.Items);
                        });
                    }
            }
            return null;
        }
    }

}
