using AForge.Controls;
using AForge.Imaging.Formats;
using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestWindowsFormsApp
{
    public partial class Form1 : Form
    {

        private ScreenCaptureStream videoSource;
        private VideoSourcePlayer videoPlayer;
        public Form1()
        {
            InitializeComponent();
            // 创建视频播放控件
            videoPlayer = new VideoSourcePlayer();
            videoPlayer.Dock = DockStyle.Fill;
            //videoPlayer.NewFrame += VideoPlayer_NewFrame;
            Rectangle a = new Rectangle();
            a = Screen.AllScreens[0].Bounds;
            a.Width = Convert.ToInt32(a.Width * 1.5);
            a.Height = Convert.ToInt32(a.Height * 1.5);
            videoSource = new ScreenCaptureStream(a, 30); // 捕捉整个屏幕，每秒 10 帧
            videoPlayer.VideoSource = videoSource;
            videoSource.NewFrame += VideoSource_NewFrame; ;
            videoPlayer.Start();
            this.Controls.Add(videoPlayer);
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var image = eventArgs.Frame;
            EncoderParameters encoderParameters = new EncoderParameters(1);
            EncoderParameter encoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);
            encoderParameters.Param[0] = encoderParameter;
            ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);

            if (jpegEncoder != null)
            {
                image.Save("xxx.jpg", jpegEncoder, encoderParameters);
                Console.WriteLine("图像压缩成功");
            }
            else
            {
                Console.WriteLine("JPEG 编码器无法找到");
            }
        }

        private void VideoPlayer_NewFrame(object sender, ref Bitmap image)
        {
            EncoderParameters encoderParameters = new EncoderParameters(1);
            EncoderParameter encoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);
            encoderParameters.Param[0] = encoderParameter;

            ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);

            if (jpegEncoder != null)
            {
                image.Save("xxx.jpg", jpegEncoder, encoderParameters);
                Console.WriteLine("图像压缩成功");
            }
            else
            {
                Console.WriteLine("JPEG 编码器无法找到");
            }


            // 在这里可以对捕捉的屏幕图像进行处理，比如进行编码传输等
            // 这里简单演示在窗口中实时显示屏幕捕捉图像
            if (videoPlayer.InvokeRequired)
            {
                videoPlayer.Invoke(new Action(() => videoPlayer.Invalidate()));
            }
            else
            {
                videoPlayer.Invalidate();
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

    }
}
