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
    public partial class Client : IClient, IDisposable
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

        public ClientRoleType GetRole() { return Role; }

        /// <summary>
        /// 连接锁
        /// </summary>
        private object ConnectLock = new object();

        /// <summary>
        /// 客户端连接成功
        /// </summary>
        public Action Client_OnConnectedSuccessfullyCallback;

        /// <summary>
        /// 服务端连接成功
        /// </summary>
        public Action Server_OnConnectedSuccessfullyCallback;

        /// <summary>
        /// 当前是否是画画模式
        /// </summary>
        public bool IsDrawing;


        public Client()
        {
            client = new TcpClient();
            server = new TcpServer();
            unpacker = new PackageUnpacker();
            client.OnConnectedSuccessfully += Client_OnConnectedSuccessfully;
            client.OnConnectionFailed += Client_OnConnectionFailed;
            unpacker.OnDataParsed += Unpacker_OnDataParsed;
            server.ClientConnected += Server_ClientConnected;
            server.Started += Server_Started;
            Plugins = new List<BasePlugin>();
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

        public void CloseClient() 
        {
            client.Disconnect();
         
        }
        public void CloseServer() 
        {
            server.CloseAsync();
       
        }

        public void Dispose()
        {
            client.Dispose();
            server.Dispose();
        }



        /// <summary>
        /// 启动受控端
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void StartServer(string ip, int port = 45555)
        {
            server.Host = ip;
            server.Port = port;
            server.StartAsync();
        }

        /// <summary>
        /// 注册组件
        /// </summary>
        /// <param name="p"></param>
        public void RegPlugin(BasePlugin p)
        {
            if (!Plugins.Any(item => item.GetType() == p.GetType()))
            {
                p.RegInit(this);
                Plugins.Add(p);
            }
        }

        protected override void ReceivedBytes(BasePackage response)
        {
          var a =  response.PluginName;
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
                    server.Clients.First().SendAsync(bytes);
                    break;
            }
        }
    }
}
