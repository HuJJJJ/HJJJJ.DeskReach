﻿using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using HJJJJ.DeskReach.Plugins.PluginMenu;
using System.Drawing;
using System.Diagnostics.Tracing;
using System.Drawing.Imaging;
using AForge.Video;

namespace HJJJJ.DeskReach.Plugins.Screen.Windows
{
    public class ScreenPlugin : BasePlugin
    {
        private const string PluginName = "HJJJJ.DeskReach.Plugins.Screen.Windows.ScreenPlugin";
        private IScreenViewContext ViewContext { get; set; }
        private new event EventHandler<byte[]> OnDataReceived;
        private AutoResetEvent AutoResetEvent = new AutoResetEvent(false);
        private Queue<byte[]> frameQueue;//接收队列
        private object queueLock;
        private int frameCount = 0;
        private DateTime lastFrameTime;
        private long Quality = 20;
        private ScreenObserver screenObserver;
        private EncoderParameters encoderParameters;
       private ImageCodecInfo jpegEncoder;
        public ScreenPlugin(IScreenViewContext viewContext)
        {
            //视频录制器
            screenObserver = new ScreenObserver(viewContext.ScreenViewBounds, VideoSource_NewFrame);
            ViewContext = viewContext;
            OnDataReceived += ScreenPlugin_OnDataReceived;
            frameQueue = new Queue<byte[]>();
            queueLock = new object();

            //图片压缩器初始化
             encoderParameters = new EncoderParameters(1);
             jpegEncoder = screenObserver.GetEncoder(ImageFormat.Jpeg);
        }



        public override void RegInit(Client client)
        {
            base.RegInit(client);
        }
        private void ScreenPlugin_OnDataReceived(object sender, byte[] e)
        {
            var screen = new ScreenPacket(e);
            switch (screen.Code)
            {
                case ScreenActionType.RequestImage:
                    screenObserver.Start();
                    break;
                case ScreenActionType.ImageFrame:
                    EnqueueFrame(screen.Image);
                    SendAck();
                    break;
                case ScreenActionType.ACK:
                    AutoResetEvent.Set();
                    break;
                case ScreenActionType.ImageQuality:
                    Quality = screen.ImageQuality;
                    break;
            }
        }


        /// <summary>
        /// 发送图片帧
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        byte[] compressed;
        private void SendFrame(byte[] bytes)
        {
            // 压缩
            using (var outputStream = new MemoryStream())
            {
                Console.WriteLine(bytes.Length);
                using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    gzipStream.Write(bytes, 0, bytes.Length);
                compressed = outputStream.ToArray();  // 压缩后的字节数组
            }
            Action(new ScreenPacket(ScreenActionType.ImageFrame, compressed));
            AutoResetEvent.WaitOne();
        }


        /// <summary>
        /// 每一帧的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var image = (Bitmap)eventArgs.Frame.Clone();
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Quality); ;

            if (jpegEncoder != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, jpegEncoder, encoderParameters);
                    image.Dispose();
                    SendFrame(ms.ToArray());
                }
            }
            else
            {
                Console.WriteLine("JPEG 编码器无法找到");
            }
        }

        ///// <summary>
        ///// 每一帧的处理
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void VideoStreamer_NewFrame(object sender, NewFrameEventArgs e)
        //{
        //    var bitmap = (Bitmap)e.Frame.Clone();
        //    var image = bitmap.GetThumbnailImage(bitmap.Width / 2, bitmap.Height / 2, null, IntPtr.Zero);
        //    bitmap.Dispose();
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        image.Save(ms, ImageFormat.Jpeg);
        //        image.Dispose();
        //        SendFrame(ms.ToArray());
        //    }
        //}

        /// <summary>
        /// 启动接收图片线程
        /// </summary>
        public void StartReceive()
        {
            Thread receiveThread = new Thread(ProcessingFramesThread);
            receiveThread.Start();
        }

        /// <summary>
        /// 将接收到的帧放入队列并返回确认帧
        /// </summary>
        /// <param name="frame"></param>
        public void EnqueueFrame(byte[] frame)
        {
            lock (queueLock)
            {
                if (frameQueue.Count > 30)
                {
                    frameQueue.Clear();//清空队列
                }
                frameQueue.Enqueue(frame);
                Monitor.Pulse(queueLock);
            }
        }

        /// <summary>
        /// 从队列中取出图片做处理
        /// </summary>
        private void ProcessingFramesThread()
        {
            byte[] frame;
            while (true)
            {
                lock (queueLock)
                {
                    while (frameQueue.Count == 0)
                    {
                        Monitor.Wait(queueLock);
                    }
                    frame = frameQueue.Dequeue();
                }
                ProcessFrame(frame);
                // SendAck();
            }
        }
        /// <summary>
        /// 处理接收到的图片
        /// </summary>
        private void ProcessFrame(byte[] frame)
        {
            // 解压缩
            using (var inputStream = new MemoryStream(frame))
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                gzipStream.CopyTo(outputStream);
                byte[] decompressed = outputStream.ToArray();  // 解压后的字节数组
                ViewContext.ShowImage(decompressed);
            }

            frameCount++;
            // 检查是否达到了三秒钟
            if (DateTime.Now - lastFrameTime >= TimeSpan.FromSeconds(1))
            {
                // 显示帧率并重置帧数和计时器
                Console.WriteLine($"接收端FPS: {frameCount}");
                frameCount = 0;
                lastFrameTime = DateTime.Now;
            }
        }

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
                    menu.AddRange(new MenuItem[] {new ComboBoxMenuItem()
                    {
                        Name = "视频质量",
                        Enable = true,
                        Items = new object[] { "Speed", "Low", "Nomal", "High", "Quality" },
                        OnIndexChanged = new Action<MenuItem,ComboBoxSelectedEvnetArgs>((sender,e) =>
                        {
                            int quality = 0;
                            switch (e.Text)
                            {
                                case "Speed":
                                    quality = 0;
                                    break;
                                case "Low":
                                    quality = 20;
                                    break;
                                case "Nomal":
                                    quality = 50;
                                    break;
                                case "High":
                                    quality = 80;
                                    break;
                                case "Quality":
                                    quality = 100;
                                    break;
                            }
                            Action(new ScreenPacket(ScreenActionType.ImageQuality, null, quality));
                        })
                    },
                    new LabelMenuItem()
                    {
                    Name ="视频质量",
                    Enable =false,
                    FontColor = Color.Gray
                    } }
                    );
                    break;
                case MenuPosition.StatusMenu:
                    break;
            }
            return menu;
        }

        private void SendAck() => Action(new ScreenPacket(ScreenActionType.ACK));

        public void Action(ScreenPacket packet) => Send(packet);


        /// <summary>
        /// 接收消息处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void RaiseDataReceived(object sender, byte[] e) => SafelyInvokeCallback(() => { OnDataReceived?.Invoke(sender, e); });
    }

}
