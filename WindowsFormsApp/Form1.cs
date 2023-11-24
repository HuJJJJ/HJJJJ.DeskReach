using AForge.Video;
using Kogel.Record;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {

    }


        public void xxx() 
        {
            InitializeComponent();
            Global.InitDllPath();
            //视频存放路径
            string recorderPath = AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.ToString("MMddHHmmss") + ".avi";
            //初始化录制器 （第一个参数是路径，第二个参数是帧数，第三个参数是是否录制声音）

            var recorder = new ScreenRecorder(recorderPath, 60, false, AForge.Video.FFMPEG.VideoCodec.H263P);
            //开始并设置每帧回调
            recorder.Start(VideoStreamer_NewFrame);
        }

        private void VideoStreamer_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var a = (Bitmap)eventArgs.Frame.Clone();
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) 
            {
                a.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                Console.WriteLine(ms.ToArray().Length);
            }
            //显示图片流
            this.pictureBox1.Image = a;
        }
    }
}
