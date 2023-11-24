using AForge.Video;
using AForge.Video.FFMPEG;
using Kogel.Record;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;
namespace HJJJJ.DeskReach.Plugins.Screen.Windows
{


    internal class ScreenObserver
    {
        /// <summary>
        /// 视频路径
        /// </summary>
        public string FilePath { get; set; } = AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.ToString("MMddHHmmss") + ".avi";
        public ScreenRecorder Recorder { get; set; }

        public ScreenObserver(string path = "")
        {
            //视频存放路径
            // FilePath = path;
            //初始化录制器 （第一个参数是路径，第二个参数是帧数，第三个参数是是否录制声音）
            Recorder = new ScreenRecorder(FilePath, 60, false, VideoCodec.H263P);
        }

        public static void InitDllPath()
        {
            Global.InitDllPath();
        }

        /// <summary>
        /// 开始并设置每帧回调
        /// </summary>
        public void Start(NewFrameEventHandler callback = null)
        {
            if (callback == null) Recorder.Start(VideoStreamer_NewFrame);
            else Recorder.Start(callback);
        }

        private void VideoStreamer_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var a = (Bitmap)eventArgs.Frame.Clone();
            using (MemoryStream ms = new MemoryStream())
            {
                a.Save(ms, ImageFormat.Jpeg);
                Console.WriteLine(ms.ToArray().Length);
            }
        }
        public static ImageCodecInfo GetEncoder(ImageFormat format)
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

        /// <summary>
        /// 暂停录制
        /// </summary>
        public void Pause() => Recorder.Pause();

        /// <summary>
        /// 结束录制
        /// </summary>
        public void End() => Recorder.End();
    }
}
