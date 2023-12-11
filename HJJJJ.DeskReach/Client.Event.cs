using HJJJJ.DeskReach.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HJJJJ.DeskReach
{
    public partial class Client
    {
        /// <summary>
        /// 连接失败时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_OnConnectionFailed(object sender, STTech.BytesIO.Core.ConnectionFailedEventArgs e)
        {
            Console.WriteLine("-----------------连接错误-----------------");
            Console.WriteLine("错误码:"+e.ErrorCode);
            Console.WriteLine("错误信息:" + e.Message);
            Console.WriteLine("错误类型:" + e.Exception.GetType().ToString());
        }



        /// <summary>
        /// 客户端连接事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_OnConnectedSuccessfully(object sender, STTech.BytesIO.Core.ConnectedSuccessfullyEventArgs e)
        {
            ///客户端主动连接，app为控制端
            lock (ConnectLock)
            {
                if (Role == ClientRoleType.None)
                {
                    client.OnDataReceived += Client_OnDataReceived;
                    Role = ClientRoleType.ControlEnd;
                }
            }
            Client_OnConnectedSuccessfullyCallback?.Invoke();
        }



        /// <summary>
        /// sd\\服务器启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_Started(object sender, EventArgs e)
        {
            Console.WriteLine("服务器启动");
        }

        /// <summary>
        /// 服务端连接事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_ClientConnected(object sender, STTech.BytesIO.Tcp.Entity.ClientConnectedEventArgs e)
        {
            ///服务端被动连接。app为受控端
            if (Role == ClientRoleType.None)
            {
                e.Client.OnDataReceived += (object s, STTech.BytesIO.Core.DataReceivedEventArgs g) => unpacker.Input(g.Data);
                Role = ClientRoleType.ControlledEnd;
            }
            Server_OnConnectedSuccessfullyCallback?.Invoke();
        }

        private void Client_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e) => unpacker.Input(e.Data);
        private void Unpacker_OnDataParsed(object sender, STTech.BytesIO.Core.Component.DataParsedEventArgs e)
        {
            BasePackage package = new BasePackage(e.Data);
            ReceivedBytes(package);
        }
    }
}
