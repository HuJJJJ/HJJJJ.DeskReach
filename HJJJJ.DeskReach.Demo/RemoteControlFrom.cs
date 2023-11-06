using HJJJJ.DeskReach.Plugins.Pointer;
using HJJJJ.DeskReach.Plugins.Screen;
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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HJJJJ.DeskReach.Demo
{
    public partial class RemoteControlFrom : Form, IPointerViewContext, IScreenViewContext
    {
        Client client;
        PointerPlugin pointer;
        ScreenPlugin screen;
        private int frameCount = 0;
        private DateTime lastFrameTime;
        private System.Windows.Forms.Label fpsLabel;
        private PictureBox pictureBox;
        public Rectangle Area { get; set; }

        public RemoteControlFrom()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            //视频显示框
            pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            panel2.Controls.Add(pictureBox);

            //帧率显示
            fpsLabel = new System.Windows.Forms.Label();
            fpsLabel.AutoSize = true;
            panel2.Controls.Add(fpsLabel);
            panel2.Controls.SetChildIndex(fpsLabel, 0);

            //构建插件
            pointer = new PointerPlugin(this);
            screen = new ScreenPlugin(this);
            client = new Client();
            client.RegPlugin(pointer);
            client.RegPlugin(screen);

            //打开本地接收
            client.StartServer();
            //连接客户端
            client.Connct(Store.TargetIP, Store.TargetPort);
            //请求传输图像
            screen.Action(new ScreenPacket(ScreenActionType.RequestImage));
            //启动图像接受
            screen.StartReceive();
        }


        private void clickMouse_Click(object sender, EventArgs e)
        {
            this.Area = Screen.GetBounds(this);
            var packet = new PointerPacket(new Point(500, 500), PointerActionType.Left);
            pointer.Action(packet);
        }

        private void screenBtn_Click(object sender, EventArgs e)
        {
            screen.Action(new ScreenPacket(ScreenActionType.RequestImage));
        }

        public byte[] GetImage()
        {
            var sendFrame = WindowsAPIScreenCapture.CaptureScreen();
            frameCount++;
            // 检查是否达到了一秒钟
            if (DateTime.Now - lastFrameTime >= TimeSpan.FromSeconds(1))
            {
                // 显示帧率并重置帧数和计时器
                fpsLabel.Text = $"FPS: {frameCount}";
                frameCount = 0;
                lastFrameTime = DateTime.Now;
            }
            return sendFrame;
        }



        public void ShowImage(byte[] image)
        {
            var tempFrame = image.BytesToBitmap();
            int targetWidth = this.pictureBox.Width;
            int targetHeight = this.pictureBox.Height;
            var frame = tempFrame.GetThumbnailImage(targetWidth, targetHeight, null, IntPtr.Zero);
            tempFrame.Dispose();
            this.pictureBox.Image = frame;
        }
    }
}
