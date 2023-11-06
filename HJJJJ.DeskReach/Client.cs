using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
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

        private TcpServer server;

        private List<BasePlugin> Plugins;

        public Client()
        {
            client = new TcpClient();
            server = new TcpServer();
            unpacker = new PackageUnpacker();
            unpacker.OnDataParsed += Unpacker_OnDataParsed;
            client.OnDataReceived += Client_OnDataReceived;
            server.ClientConnected += Server_ClientConnected;
            Plugins = new List<BasePlugin>();
        }

        private void Server_ClientConnected(object sender, STTech.BytesIO.Tcp.Entity.ClientConnectedEventArgs e)
        {
            e.Client.OnDataReceived += (object s, STTech.BytesIO.Core.DataReceivedEventArgs g) => unpacker.Input(g.Data);
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="timeout"></param>
        public void Connct(string host,int port, int timeout = 10)
        {
            client.Host = host;
            client.Port = port;
            client.Connect(timeout);
        }

        public void StartServer(int port = 45555)
        {
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
                        x.RaiseDataReceived(this, response.DetailData);
                });
        }


        protected override void SendBytes(byte[] bytes)
        {
            client.SendAsync(bytes);
        }
    }
}
