using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using HJJJJ.DeskReach.Plugins.PluginMenu;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using HJJJJ.DeskReach.Plugins.Screen.Windows.Package;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;
using System.Collections;

namespace HJJJJ.DeskReach.Plugins.Screen.Windows
{
    public class ScreenPlugin : BasePlugin
    {
        private const string PluginName = "HJJJJ.DeskReach.Plugins.Screen.Windows.ScreenPlugin";

        public bool IsEnabled { get; set; }
        private IScreenViewContext ViewContext { get; set; }
        private new event EventHandler<byte[]> OnDataReceived;
        private AutoResetEvent AutoResetEvent = new AutoResetEvent(false);
        private Queue<UpdateImageRequest> frameQueue;//接收队列
        private object queueLock; //队列锁
        private static object updateImage; //更新图片锁
        private Bitmap newBitmap; //旧图片
        private Bitmap oldBitmap;//新图片
        private WindowsAPIScreenCapture screenCapture; //屏幕工具类
        private System.Timers.Timer timer; //更新帧计时器
        private long ImageFrameTimeStamp = 0;
        public ScreenPlugin(IScreenViewContext viewContext)
        {
            IsEnabled = true;
            ViewContext = viewContext;
            this.OnDataReceived += ScreenPlugin_OnDataReceived;
            frameQueue = new Queue<UpdateImageRequest>();
            queueLock = new object();
            updateImage = new object();
            timer = new System.Timers.Timer();
        }



        public override void RegInit(Client client)
        {
            base.RegInit(client);

            //初始化bitmap和截图工具
            if (client.GetRole() == Entities.ClientRoleType.ControlledEnd) 
            {
                //一秒发送30帧
                timer.Interval = 1000 / 30;
                timer.Elapsed += SendFrame;
                //获取缩放比例
                screenCapture = new WindowsAPIScreenCapture();
                var scalingFactor = screenCapture.GetScreenScalingFactor();
                var width = Convert.ToInt32(ViewContext.ScreenViewBounds.Width * scalingFactor);
                var height = Convert.ToInt32(ViewContext.ScreenViewBounds.Height * scalingFactor);
                newBitmap = new Bitmap(width / 2, height / 2, PixelFormat.Format24bppRgb);
                oldBitmap = new Bitmap(width / 2, height / 2, PixelFormat.Format24bppRgb);
                screenCapture.Init(oldBitmap);
            }
        }
        private void ScreenPlugin_OnDataReceived(object sender, byte[] e)
        {
            var code = (ScreenActionType)e.First();

            switch (code)
            {
                case ScreenActionType.UpdateImage:
                    var updateImage = new UpdateImageRequest(e);
                    EnqueueFrame(updateImage);
                    SendAck();
                    break;
                case ScreenActionType.RequestImageRequest:
                    timer.Start();
                    Action(new RequestImageResponse(newBitmap.Width, newBitmap.Height));
                    break;
                case ScreenActionType.RequestImageResponse:
                    var response = new RequestImageResponse(e);
                    newBitmap = new Bitmap(response.Width, response.Height, PixelFormat.Format24bppRgb);
                    ViewContext.InitBitmap(response.Width, response.Height, PixelFormat.Format24bppRgb);
                    break;
                case ScreenActionType.ACK:
                    AutoResetEvent.Set();
                    break;
                default:
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
        object obj = new object();
        private void SendFrame(object sender, ElapsedEventArgs e)
        {
            screenCapture.CaptureScreen();
            lock (obj)
            {
                int heightBlockSize = oldBitmap.Height / 10;
                long timeStamp = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
                for (int y = 0; y < oldBitmap.Height; y += heightBlockSize)
                {
                    var rectangle = new Rectangle(0, y, oldBitmap.Width, heightBlockSize);
                    var bytes = BitmapMemoryIO.ReadBitmapBlock(oldBitmap, rectangle);
                    var result = BitmapMemoryIO.CompareDifference(newBitmap, rectangle, bytes);
                    if (result) continue;
                    BitmapMemoryIO.WriteBitmapBlock(newBitmap, rectangle, bytes);
                    Action(new UpdateImageRequest(0, y, bytes, heightBlockSize, oldBitmap.Width, timeStamp));
                }
            }

            //// 压缩
            //using (var outputStream = new MemoryStream())
            //{
            //    Console.WriteLine(bytes.Length);
            //    using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            //        gzipStream.Write(bytes, 0, bytes.Length);
            //    compressed = outputStream.ToArray();  // 压缩后的字节数组
            //}
            //Action(new ScreenPacket(ScreenActionType.ImageFrame, compressed));
            //AutoResetEvent.WaitOne();

        }




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
        public void EnqueueFrame(UpdateImageRequest frame)
        {
            lock (queueLock)
            {
                //if (frameQueue.Count > 30)
                //{
                //    frameQueue.Clear();//清空队列
                //}
                frameQueue.Enqueue(frame);
                Monitor.Pulse(queueLock);
            }
        }

        /// <summary>
        /// 从队列中取出图片做处理
        /// </summary>
        private void ProcessingFramesThread()
        {
            //UpdateImageRequest frame = null;
            List<UpdateImageRequest> list = new List<UpdateImageRequest>();
            while (true)
            {
                lock (queueLock)
                {
                    while (frameQueue.Count == 0)
                    {
                        Monitor.Wait(queueLock);
                    }

                    while (frameQueue.Count > 0)
                        list.Add(frameQueue.Dequeue());
                    // frame = frameQueue.Dequeue();
                }
                ProcessFrame(list);
                list.Clear();
                // SendAck();
            }
        }


        /// <summary>
        /// 处理接收到的图片
        /// </summary>
        private void ProcessFrame(List<UpdateImageRequest> frameList)
        {
            IOrderedEnumerable<UpdateImageRequest> groupedObjects = null;
            //如果当前没有更新的帧，则取出队列中时间戳最接近现在的帧组
      
                groupedObjects = frameList.GroupBy(obj => obj.TimeStamp)
                                         .OrderByDescending(group => group.Key)
                                         .Select(group => group.OrderBy(obj => obj.TimeStamp)).First();
            lock (updateImage)
            {
                foreach (var image in groupedObjects)
                {
                    BitmapMemoryIO.WriteBitmapBlock(newBitmap, new Rectangle(image.StartX, image.StartY, image.WidthBlockSize, image.HeightBlockSize), image.Image);
                    ViewContext.ShowImage((Bitmap)newBitmap.Clone());
                }
            }

            //// 解压缩
            //using (var inputStream = new MemoryStream(frame))
            //using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            //using (var outputStream = new MemoryStream())
            //{
            //    gzipStream.CopyTo(outputStream);
            //    byte[] decompressed = outputStream.ToArray();  // 解压后的字节数组
            //    ViewContext.ShowImage(decompressed);
            //}
        }

        public override List<PluginMenu.MenuItem> GetMenuItems(MenuPosition pos)
        {
            var menu = new List<PluginMenu.MenuItem>();
            switch (pos)
            {
                case MenuPosition.TopMenu:
                    break;
                case MenuPosition.Screen:
                    break;
                case MenuPosition.ToolBar:
                    break;
                case MenuPosition.ToolMenu:
                    break;
                case MenuPosition.StatusMenu:
                    break;
            }
            return menu;
        }

        private void SendAck() => Action(new ACKRequest());

        public void Action(ScreenPacket packet) => Send(packet);


        /// <summary>
        /// 接收消息处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void RaiseDataReceived(object sender, byte[] e) => SafelyInvokeCallback(() => { OnDataReceived?.Invoke(sender, e); });
    }

}
