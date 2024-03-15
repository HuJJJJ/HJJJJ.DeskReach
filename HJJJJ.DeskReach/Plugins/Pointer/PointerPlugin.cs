using HJJJJ.DeskReach.Plugins.Keyboard;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Pointer
{
    public class PointerPlugin : BasePlugin
    {
        private new event EventHandler<byte[]> OnDataReceived;
        private IPointerViewContext ViewContext { get; set; }
        public bool IsEnabled { get; set; }
        public PointerPlugin(IPointerViewContext viewContext)
        {
            IsEnabled = false;
            ViewContext = viewContext;
            this.OnDataReceived += PointerPlugin_OnReceived;
        }

        /// <summary>
        /// 接收消息处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void RaiseDataReceived(object sender, byte[] e) => SafelyInvokeCallback(() => { OnDataReceived?.Invoke(sender, e); });

        /// <summary>
        /// 操作鼠标
        /// </summary>
        public void Action(BasePacket packet)
        {
            if (this.IsEnabled)
                Send(packet);
        }

        /// <summary>
        /// 禁用插件
        /// </summary>
        public override void DisablePlugin()
        {
            IsEnabled = false;
            Action(new PointerPacket(new Point(0, 0), PointerActionType.Disable));
        }

        public override void EnablePlugin()
        {
            IsEnabled = true;
            Action(new PointerPacket(new Point(0, 0), PointerActionType.Enable));
        }


        private void PointerPlugin_OnReceived(object sender, byte[] bytes)
        {
            var pointer = new PointerPacket(bytes);
            //按屏幕比例转换鼠标位置
            var width = Convert.ToInt32((double)(pointer.X / 100.0) * ViewContext.PointerViewBounds.Width);
            var height = Convert.ToInt32((double)(pointer.Y / 100.0) * ViewContext.PointerViewBounds.Height);
            //移动鼠标
            mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, width * 65535 / ViewContext.PointerViewBounds.Width, height * 65535 / ViewContext.PointerViewBounds.Height, 0, 0);


            switch (pointer.Code)
            {
                case PointerActionType.LeftDown:
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    break;
                case PointerActionType.LeftUp:
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;
                case PointerActionType.DoubleLeftClick:
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;
                case PointerActionType.RightDown:
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                    break;
                case PointerActionType.RightUp:
                    mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;
                case PointerActionType.DoubleRightClick:
                    mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;
                case PointerActionType.MiddleDown:
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                    break;
                case PointerActionType.MiddleUp:
                    mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                    break;
                case PointerActionType.MiddleWheel:
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, pointer.Wheel, 0);
                    break;
                case PointerActionType.DoubleMiddleClick:
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN | MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN | MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                    break;
                case PointerActionType.Move:
                    // mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, width * 65535 / ViewContext.PointerViewBounds.Width, height * 65535 / ViewContext.PointerViewBounds.Height, 0, 0);
                    break;
                case PointerActionType.XButton1Down:
                    mouse_event(MOUSEEVENTF_XDOWN, 0, 0, XBUTTON1, 0);
                    break;
                case PointerActionType.XButton1Up:
                    mouse_event(MOUSEEVENTF_XUP, 0, 0, XBUTTON1, 0);
                    break;
                case PointerActionType.DoubleXButton1Click:
                    mouse_event(MOUSEEVENTF_XDOWN | MOUSEEVENTF_XUP, 0, 0, XBUTTON1, 0);
                    mouse_event(MOUSEEVENTF_XDOWN | MOUSEEVENTF_XUP, 0, 0, XBUTTON1, 0);
                    break;
                case PointerActionType.XButton2Down:
                    mouse_event(MOUSEEVENTF_XDOWN, 0, 0, XBUTTON2, 0);
                    break;
                case PointerActionType.XButton2Up:
                    mouse_event(MOUSEEVENTF_XUP, 0, 0, XBUTTON2, 0);
                    break;
                case PointerActionType.DoubleXButton2Click:
                    mouse_event(MOUSEEVENTF_XDOWN | MOUSEEVENTF_XUP, 0, 0, XBUTTON2, 0);
                    mouse_event(MOUSEEVENTF_XDOWN | MOUSEEVENTF_XUP, 0, 0, XBUTTON2, 0);
                    break;
                case PointerActionType.None:
                default:
                case PointerActionType.Enable:
                    this.IsEnabled = true;
                    break;
                case PointerActionType.Disable:
                    this.IsEnabled = false;
                    break;
            }
            //switch (pointer.Code)
            //{
            //    case PointerActionType.Down:
            //        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            //        break;
            //    case PointerActionType.DoubleLeftClick:
            //        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            //        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            //        break;
            //    case PointerActionType.RightClick:
            //        mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
            //        break;
            //    case PointerActionType.DoubleRightClick:
            //        mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
            //        mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
            //        break;
            //    case PointerActionType.MiddleClick:
            //        mouse_event(MOUSEEVENTF_MIDDLEDOWN | MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
            //        break;
            //    case PointerActionType.Wheel:
            //        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, pointer.Wheel, 0);
            //        break;
            //    default:
            //        break;
            //}
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
        // 鼠标侧键按下
        const int MOUSEEVENTF_XDOWN = 0x0080;
        // 鼠标侧键释放
        const int MOUSEEVENTF_XUP = 0x0100;
        // 第一个侧键
        const int XBUTTON1 = 0x0001;
        // 第二个侧键
        const int XBUTTON2 = 0x0002;
    }
}
