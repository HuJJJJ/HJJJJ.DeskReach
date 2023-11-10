using HJJJJ.DeskReach.Plugins.Keyboard;
using HJJJJ.DeskReach.Plugins.Pointer;
using HJJJJ.DeskReach.Plugins.Screen;
using HJJJJ.DeskReach.Plugins.TextMessage.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HJJJJ.DeskReach.Demo
{
    public class Store
    {
        public static string TargetIP { get; set; }
        public static int TargetPort { get; set; }

        public static Client client;
        public static PointerPlugin pointer;
        public static ScreenPlugin screen;
        public static TextMessagePlugin textMessage;
        public static KeyboardPlugin keyboard;

        /// <summary>
        /// 构建插件
        /// </summary>
        public static void BuidingPlugin(IPointerViewContext pointerView, IScreenViewContext screenView, ITextMessageViewContext textMessageView, IKeyboardViewContext keyboardView)
        {
            //构建插件并注册
            pointer = new PointerPlugin(pointerView);
            screen = new ScreenPlugin(screenView);
            textMessage = new TextMessagePlugin(textMessageView);
            keyboard = new KeyboardPlugin(keyboardView);
            client = new Client();
            client.RegPlugin(pointer);
            client.RegPlugin(screen);
            client.RegPlugin(textMessage);
            client.RegPlugin(keyboard);
        }

        /// <summary>
        /// 客户端连接
        /// </summary>
        public static void ClientConnect()
        {
            client.Connct(TargetIP, TargetPort);
        }

        /// <summary>
        /// 启动本地服务
        /// </summary>
        public static void ServerConnect(string ip, int port)
        {
            client.StartServer(ip, port);
        }
    }
}
