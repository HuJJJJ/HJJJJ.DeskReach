using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using HJJJJ.DeskReach.Plugins.PluginMenu;
using System.Drawing;

namespace HJJJJ.DeskReach.Plugins.Screen
{
    public class ScreenPlugin : BasePlugin, IDisposable
    {
        private const string PluginName = "HJJJJ.DeskReach.Plugins.Screen.ScreenPlugin";
        private IScreenViewContext ViewContext { get; set; }
        private volatile bool isEnabled;
        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }

        private new event EventHandler<byte[]> OnDataReceived;
        private readonly System.Timers.Timer timer = new System.Timers.Timer();
        private readonly object sendFrameLock = new object();
        private readonly object queueLock = new object();
        // 正常情况发送端等待重建 ACK，只有一个在途更新；超时后用关键帧恢复。
        private byte[] pendingFrame;
        private byte[] waitingAcknowledgement;
        private long sentAt;
        private CancellationTokenSource receiveCancellation;
        private volatile bool disposed;
        private int frameCount = 0;
        private DateTime lastFrameTime;

        private int Quality=50;
        public ScreenPlugin(IScreenViewContext viewContext)
        {
            IsEnabled = true;
            ViewContext = viewContext;
            OnDataReceived += ScreenPlugin_OnDataReceived;
            timer.Interval = 1000 / 40;
            timer.Elapsed += SendFrame;
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
                    lock (sendFrameLock)
                    {
                        if (IsEnabled && !disposed)
                        {
                            waitingAcknowledgement = null;
                            ViewContext.RequestKeyFrame();
                            timer.Start();
                        }
                    }
                    break;
                case ScreenActionType.ImageFrame:
                    EnqueueFrame(screen.Image);
                    break;
                case ScreenActionType.ACK:
                    lock (sendFrameLock)
                    {
                        if (Matches(waitingAcknowledgement, screen.Image)) waitingAcknowledgement = null;
                    }
                    break;
                case ScreenActionType.RequestKeyFrame:
                    lock (sendFrameLock)
                    {
                        waitingAcknowledgement = null;
                        ViewContext.RequestKeyFrame();
                    }
                    break;
                case ScreenActionType.ImageQuality:
                    lock (sendFrameLock)
                    {
                        if (disposed) break;
                        Quality = Math.Max(1, Math.Min(100, screen.ImageQuality));
                        // 静止桌面也需要按新画质重发全屏；保留在途 ACK 的限制。
                        ViewContext.RequestKeyFrame();
                    }
                    break;
            }
        }



        /// <summary>
        /// 发送图片帧
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void SendFrame(object sender, ElapsedEventArgs e)
        {
            // TryEnter 不会排队等待：上一帧没结束，本次定时触发直接跳过。
            if (!IsEnabled || !Monitor.TryEnter(sendFrameLock)) return;
            try
            {
                if (!IsEnabled || client == null || client.GetRole() != Entities.ClientRoleType.ControlledEnd)
                {
                    timer.Stop();
                    return;
                }
                if (waitingAcknowledgement != null)
                {
                    if ((Stopwatch.GetTimestamp() - sentAt) < Stopwatch.Frequency * 2L) return;
                    // ACK 丢失/处理超时后不能继续发依赖旧状态的差量，改发关键帧。
                    waitingAcknowledgement = null;
                    ViewContext.RequestKeyFrame();
                }
                var input = ViewContext.GetImage(Quality);
                // DXGI 桌面未变化时可能没有新帧。
                if (input == null || input.Length == 0) return;
                waitingAcknowledgement = ScreenFrame.ReadHeader(input).GetAcknowledgement();
                sentAt = Stopwatch.GetTimestamp();
                Action(new ScreenPacket(ScreenActionType.ImageFrame, input));
            }
            catch (Exception ex)
            {
                Console.WriteLine("发送屏幕帧失败: " + ex.Message);
                waitingAcknowledgement = null;
                ViewContext.RequestKeyFrame();
            }
            finally
            {
                Monitor.Exit(sendFrameLock);
            }
        }

        /// <summary>
        /// 启动接收图片线程
        /// </summary>
        public void StartReceive()
        {
            lock (queueLock)
            {
                if (!IsEnabled || disposed || (receiveCancellation != null && !receiveCancellation.IsCancellationRequested)) return;
                var cancellation = new CancellationTokenSource();
                receiveCancellation = cancellation;
                var receiveThread = new Thread(() => ProcessingFramesThread(cancellation));
                receiveThread.IsBackground = true;
                receiveThread.Name = "DeskReach screen receiver";
                receiveThread.Start();
            }
        }

        /// <summary>
        /// ACK 控制下最多一个待处理更新。超时替换或缺帧由重建器验证基准并请求关键帧。
        /// </summary>
        /// <param name="frame"></param>
        public void EnqueueFrame(byte[] frame)
        {
            lock (queueLock)
            {
                if (!IsEnabled) return;
                pendingFrame = frame;
                Monitor.Pulse(queueLock);
            }
        }

        /// <summary>
        /// 从队列中取出图片做处理
        /// </summary>
        private void ProcessingFramesThread(CancellationTokenSource cancellation)
        {
            var token = cancellation.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    byte[] frame;
                    lock (queueLock)
                    {
                        while (pendingFrame == null && !token.IsCancellationRequested)
                            Monitor.Wait(queueLock);
                        if (token.IsCancellationRequested) return;
                        frame = pendingFrame;
                        pendingFrame = null;
                    }
                    ProcessFrame(frame, token);
                }
            }
            finally
            {
                lock (queueLock)
                {
                    if (ReferenceEquals(receiveCancellation, cancellation)) receiveCancellation = null;
                    cancellation.Dispose();
                }
            }
        }
        /// <summary>
        /// 处理接收到的图片
        /// </summary>
        private void ProcessFrame(byte[] frame, CancellationToken token)
        {
            try
            {
                var header = ScreenFrame.ReadHeader(frame);
                if (token.IsCancellationRequested) return;
                ViewContext.ShowImage(frame);
                // 必须在差量已合成完整画面之后确认，不能在入队时就确认。
                if (!token.IsCancellationRequested && IsEnabled && client != null && client.GetRole() == Entities.ClientRoleType.ControlEnd)
                    Action(new ScreenPacket(ScreenActionType.ACK, header.GetAcknowledgement()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(  ex.Message);
                if (!token.IsCancellationRequested && IsEnabled && client != null && client.GetRole() == Entities.ClientRoleType.ControlEnd)
                    Action(new ScreenPacket(ScreenActionType.RequestKeyFrame));
                return;
            }
            frameCount++;
            // 每秒统计处理帧数；实际窗口绘制帧率需在 UI 中另外测量。
            if (DateTime.Now - lastFrameTime >= TimeSpan.FromSeconds(1))
            {
                // 显示帧率并重置帧数和计时器
                Console.WriteLine($"接收端FPS: {frameCount}");
                frameCount = 0;
                lastFrameTime = DateTime.Now;
            }
        }

        public override void EnablePlugin() => IsEnabled = !disposed;

        public override void DisablePlugin()
        {
            IsEnabled = false;
            lock (sendFrameLock)
            {
                waitingAcknowledgement = null;
                if (!disposed) timer.Stop();
            }
            lock (queueLock)
            {
                pendingFrame = null;
                receiveCancellation?.Cancel();
                Monitor.PulseAll(queueLock);
            }
        }

        public void Dispose()
        {
            DisablePlugin();
            lock (sendFrameLock)
            {
                if (disposed) return;
                disposed = true;
                timer.Elapsed -= SendFrame;
                timer.Dispose();
            }
        }

        private static bool Matches(byte[] expected, byte[] actual)
        {
            if (expected == null || actual == null || expected.Length != actual.Length) return false;
            for (int i = 0; i < expected.Length; i++) if (expected[i] != actual[i]) return false;
            return true;
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
                                    quality = 1; // TurboJPEG 质量范围为 1～100。
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

        public void Action(ScreenPacket packet) => Send(packet);


        /// <summary>
        /// 接收消息处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void RaiseDataReceived(object sender, byte[] e) => SafelyInvokeCallback(() => { OnDataReceived?.Invoke(sender, e); });
    }

}
