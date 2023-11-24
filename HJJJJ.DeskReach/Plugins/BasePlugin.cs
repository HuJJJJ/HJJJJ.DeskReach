using HJJJJ.DeskReach.Plugins.Keyboard;
using HJJJJ.DeskReach.Plugins.PluginMenu;
using HJJJJ.DeskReach.Plugins.Pointer;
using HJJJJ.DeskReach.Plugins.Screen;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace HJJJJ.DeskReach.Plugins
{
    public class BasePlugin
    {
       public Client client;


        public virtual List<MenuItem> GetMenuItems(MenuPosition pos)
        {
            //switch (pos)
            //{
            //    case MenuPosition.TopMenu:
            //        {
            //            return new MenuItem[]
            //            {
            //                new ButtonMenuItem() {Name = "高清" },
            //                new ButtonMenuItem() {Name = "标清" },
            //                new ButtonMenuItem() {Name = "流畅" },
            //            };
            //        }
            //    case MenuPosition.Screen:
            //        break;
            //    case MenuPosition.ToolBar:
            //        break;
            //}
            return null;
        }


        public virtual void RegInit(Client client)
        {
            this.client = client;
        }
        /// <summary>
        /// 在接收到数据时发生
        /// </summary>
        public event EventHandler<byte[]> OnDataReceived;


        protected virtual void OnLoad(EventArgs e)
        {
        }



        /// <summary>
        /// 执行接收数据回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void RaiseDataReceived(object sender, byte[] e)
        {

        }

        protected void Send(BasePacket packet)
        {
            var bytes = new BasePackage(packet.PluginName, packet.GetBytes()).GetBytes();
            client.Send(bytes);
        }

        /// <summary>
        /// 执行回调事件
        /// </summary>
        /// <param name="action"></param>
        public void SafelyInvokeCallback(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Console.WriteLine("错误--------------------------------：" + e.Message + "-------------------------------------------");
                throw;
            }
        }
    }


}

