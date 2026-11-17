using System;
using System.Collections.Generic;
using System.Linq;
using HJJJJ.DeskReach.Entities;
using HJJJJ.DeskReach.Plugins;
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
        public TcpClient client;

        /// <summary>
        /// TCPClient
        /// </summary>
        public TcpServer server;

        private List<BasePlugin> Plugins;

        /// <summary>
        /// 是否是控制端
        /// </summary>
        private ClientRoleType Role;

        public ClientRoleType GetRole() => Role;

        /// <summary>
        /// 当前是否是画画模式
        /// </summary>
        public bool IsDrawing;


        public Client()
        {
            client = new TcpClient();
            client.ReceiveBufferSize = 3000000;
            server = new TcpServer();
            unpacker = new PackageUnpacker();
            unpacker.OnDataParsed += Unpacker_OnDataParsed;
            client.OnConnectedSuccessfully += Client_OnConnectedSuccessfully;
            client.OnConnectionFailed += Client_OnConnectionFailed;
            client.OnDisconnected += Client_OnDisconnected;
            server.ClientConnected += Server_ClientConnected;
            server.Started += Server_Started;
            server.Closed += Server_Closed;
            Plugins = new List<BasePlugin>();
        }






        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="timeout"></param>
        public void Connct(string host, int port, int timeout = 5000)
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
            foreach (var plugin in Plugins.ToArray())
            {
                if (plugin is IDisposable disposable) disposable.Dispose();
                else plugin.DisablePlugin();
            }
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
                p.EnablePlugin();
         //       p.EnablePlugin();
            }
        }

        /// <summary>
        /// 卸载组件
        /// </summary>
        /// <param name="p"></param>
        public void UnregPlugin(BasePlugin p)
        {
            var plugin = Plugins.Where(item => item.GetType() == p.GetType()).FirstOrDefault();
            if (plugin != null)
            {
                plugin.DisablePlugin();
                plugin.UnregClient();
                Plugins.Remove(plugin);
            }
        }


        protected override void ReceivedBytes(BasePackage response)
        {
            var a = response.PluginName;
            Plugins
                .ForEach(x =>
                {
                    if (x.GetType().ToString() == response.PluginName)
                    {
                        x.RaiseDataReceived(this, response.DetailData);
                        return;
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
