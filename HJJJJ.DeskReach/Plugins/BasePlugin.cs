using HJJJJ.DeskReach.Plugins.Keyboard;
using HJJJJ.DeskReach.Plugins.Pointer;
using HJJJJ.DeskReach.Plugins.Screen;
using System;
using System.Collections.Generic;
using System.Text;

namespace HJJJJ.DeskReach.Plugins
{
    public class BasePlugin
    {


        IClient client;
        internal void RegInit(IClient client)
        {
            this.client = client;
        }
        /// <summary>
        /// 在接收到数据时发生
        /// </summary>
        public  event EventHandler<byte[]> OnDataReceived;


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

