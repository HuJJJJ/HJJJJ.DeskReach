using HJJJJ.DeskReach.Plugins.Keyboard;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Pointer
{
    public class PointerPlugin : BasePlugin
    {
        public new event EventHandler<byte[]> OnDataReceived;
        private IPointerViewContext ViewContext { get; set; }

        public PointerPlugin(IPointerViewContext viewContext)
        {
            ViewContext = viewContext;
            this.OnDataReceived += PointerPlugin_OnReceived;
        }

        /// <summary>
        /// 操作鼠标
        /// </summary>
        public void Action(PointerPacket packet) => Send(packet);

        /// <summary>
        /// 接收消息处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void RaiseDataReceived(object sender, byte[] e) => SafelyInvokeCallback(() => { OnDataReceived?.Invoke(sender, e); });

        private void PointerPlugin_OnReceived(object sender, byte[] bytes)
        {
            var pointer = new PointerPacket(bytes);
            //按屏幕比例转换鼠标位置
            var width = Convert.ToInt32((double)(pointer.X / 100.0) * ViewContext.Area.Width);
            var height = Convert.ToInt32((double)(pointer.Y / 100.0) * ViewContext.Area.Height);
            //移动鼠标
            mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, width * 65535 / ViewContext.Area.Width, height * 65535 / ViewContext.Area.Height, 0, 0);

            switch (pointer.Code)
            {
                case PointerActionType.Left:
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;
                case PointerActionType.DoubleLeft:
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;
                case PointerActionType.Right:
                    mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;
                case PointerActionType.DoubleRight:
                    mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;
                case PointerActionType.Middle:
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN | MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                    break;
                case PointerActionType.MouseWheel:
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, pointer.Wheel, 0);
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// 鼠标模拟器
        /// </summary>
        /// <param name="dwFlags"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="dwData"></param>
        /// <param name="dwExtraInfo"></param>
        /// <returns></returns>
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        //移动鼠标 
        const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        //模拟鼠标滚轮滚动操作，必须配合dwData参数
        const int MOUSEEVENTF_WHEEL = 0x0800;
    }
}
