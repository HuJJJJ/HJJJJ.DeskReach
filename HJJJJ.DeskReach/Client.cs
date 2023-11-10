using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using HJJJJ.DeskReach.Entities;
using HJJJJ.DeskReach.Plugins;
using HJJJJ.DeskReach.Plugins.Keyboard;
using HJJJJ.DeskReach.Plugins.Pointer;
using HJJJJ.DeskReach.Plugins.Screen;
using STTech.BytesIO.Tcp;

namespace HJJJJ.DeskReach
{
    public class Client : IClient
    {
        /// <summary>
        /// 解包器
        /// </summary>
        private PackageUnpacker unpacker;

        /// <summary>
        /// TCPClient
        /// </summary>
        private TcpClient client;

        /// <summary>
        /// TCPClient
        /// </summary>
        private TcpServer server;

        private List<BasePlugin> Plugins;

        /// <summary>
        /// 是否是控制端
        /// </summary>
        private ClientRoleType Role;

        /// <summary>
        /// 连接锁
        /// </summary>
        private object ConnectLock = new object();

        public Client()
        {
            client = new TcpClient();
            client.OnConnectedSuccessfully += Client_OnConnectedSuccessfully;
            server = new TcpServer();
            unpacker = new PackageUnpacker();
            unpacker.OnDataParsed += Unpacker_OnDataParsed;
            server.ClientConnected += Server_ClientConnected;
            Plugins = new List<BasePlugin>();
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
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="timeout"></param>
        public void Connct(string host, int port, int timeout = 10)
        {
            client.Host = host;
            client.Port = port;
            client.Connect(timeout);
        }

        public void StartServer(string ip, int port = 45555)
        {
            server.Host = ip;
            server.Port = port;
            server.StartAsync();
        }

        public void RegPlugin(BasePlugin p)
        {
            if (!Plugins.Any(item => item.GetType() == p.GetType()))
            {
                p.RegInit(this);
                Plugins.Add(p);
            }
        }

        private void Client_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e) => unpacker.Input(e.Data);
        private void Unpacker_OnDataParsed(object sender, STTech.BytesIO.Core.Component.DataParsedEventArgs e)
        {
            BasePackage package = new BasePackage(e.Data);
            ReceivedBytes(package);
        }

        protected override void ReceivedBytes(BasePackage response)
        {
            Plugins
                .ForEach(x =>
                {
                    if (x.GetType().ToString() == response.PluginName)
                    {
                        x.RaiseDataReceived(this, response.DetailData);
                    }
                });
        }

        protected override void SendBytes(byte[] bytes)
        {
            switch (Role)
            {
                case ClientRoleType.None:
                    break;
                case ClientRoleType.ControlEnd:
                    client.SendAsync(bytes);
                    break;
                case ClientRoleType.ControlledEnd:
                    server.Clients.First().Send(bytes);
                    break;
            }
          
        }
    }
}
