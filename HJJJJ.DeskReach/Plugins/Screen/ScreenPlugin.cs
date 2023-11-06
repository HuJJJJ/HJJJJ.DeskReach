using HJJJJ.DeskReach.Plugins.Pointer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;

namespace HJJJJ.DeskReach.Plugins.Screen
{
    public class ScreenPlugin : BasePlugin
    {
        public IScreenViewContext ViewContext { get; set; }
        public new event EventHandler<byte[]> OnDataReceived;
        private AutoResetEvent AutoResetEvent = new AutoResetEvent(false);
        System.Timers.Timer timer = new System.Timers.Timer();
        private Queue<byte[]> frameQueue;//接收队列
        private object queueLock;


        public ScreenPlugin(IScreenViewContext viewContext)
        {
            ViewContext = viewContext;
            OnDataReceived += ScreenPlugin_OnDataReceived;
            frameQueue = new Queue<byte[]>();
            queueLock = new object();
            timer.Interval = 1000 / 40;
            timer.Elapsed += SendFrame;
        }
        private int frameCount = 0;
        private DateTime lastFrameTime;
        private void ScreenPlugin_OnDataReceived(object sender, byte[] e)
        {
            var screen = new ScreenPacket(e);
            switch (screen.Code)
            {
                case ScreenActionType.RequestImage:
                    timer.Start();
                    break;
                case ScreenActionType.ImageFrame:
                    EnqueueFrame(screen.Image);
                    SendAck();
                    break;
                case ScreenActionType.ImageACKFrame:
                    AutoResetEvent.Set();
                    break;
                case ScreenActionType.ImageQuality:
                    break;
            }
        }



        /// <summary>
        /// 发送图片帧
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendFrame(object sender, ElapsedEventArgs e)
        {
            Action(new ScreenPacket(ScreenActionType.ImageFrame, ViewContext.GetImage()));
            AutoResetEvent.WaitOne();
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
        public void EnqueueFrame(byte[] frame)
        {
            lock (queueLock)
            {
                frameQueue.Enqueue(frame);
                Monitor.Pulse(queueLock);
            }
        }

        /// <summary>
        /// 从队列中取出图片做处理
        /// </summary>
        private void ProcessingFramesThread()
        {
            while (true)
            {
                byte[] frame;

                lock (queueLock)
                {
                    while (frameQueue.Count == 0)
                    {
                        Monitor.Wait(queueLock);
                    }

                    frame = frameQueue.Dequeue();
                }
                ProcessFrame(frame);
            }
        }

        /// <summary>
        /// 处理接收到的图片
        /// </summary>
        private void ProcessFrame(byte[] frame)
        {
            ViewContext.ShowImage(frame);
            frameCount++;
            // 检查是否达到了三秒钟
            if (DateTime.Now - lastFrameTime >= TimeSpan.FromSeconds(3))
            {
                // 显示帧率并重置帧数和计时器
                Console.WriteLine($"接收端FPS: {frameCount}");
                frameCount = 0;
                lastFrameTime = DateTime.Now;
                lock (queueLock) frameQueue.Clear();//清空队列
            }
        }

        private void SendAck() => Action(new ScreenPacket(ScreenActionType.ImageACKFrame));

        public void Action(ScreenPacket packet) => Send(packet);


        /// <summary>
        /// 接收消息处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void RaiseDataReceived(object sender, byte[] e) => SafelyInvokeCallback(() => { OnDataReceived?.Invoke(sender, e); });
    }

}
