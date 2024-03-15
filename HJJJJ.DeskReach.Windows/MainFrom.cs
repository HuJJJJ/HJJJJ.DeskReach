using ApeFree.ApeDialogs;
using ApeFree.ApeDialogs.Settings;
using ApeFree.ApeForms.Core.Controls;
using ApeFree.ApeForms.Forms.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Windows.Media.Protection.PlayReady;

namespace HJJJJ.DeskReach.Windows
{
    public partial class MainFrom : Form
    {
        /// <summary>
        /// ApeForms
        /// </summary>
        ApeFormsDialogProvider provider = DialogFactory.Factory.GetApeFormsDialogProvider();
        private static string TargetIP { get; set; }
        private static int TargetPort { get; set; }


        private readonly int LocalPort = 45555;
        private ConnectionMode ConnMode { get; set; } //连接模式

        public MainFrom()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            //获取ip地址
            string hostName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
            var addresses = hostEntry.AddressList.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && (x.ToString().StartsWith("192.168.") || x.ToString().StartsWith("10.")));
            comboBox.Items.AddRange(addresses.ToArray());
            comboBox.SelectedIndex = 0;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;

        }

        /// <summary>
        /// 子窗体关闭后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            var client = sender as Client;
            client.CloseClient();
            client.CloseServer();
            client.Dispose();
            this.Show();
        }

        /// <summary>
        /// 选择连接模式并连接远端设备
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {

            // 创建 ConnectionType 枚举的列表
            var connectionTypes = Enum.GetValues(typeof(ConnectionType)).Cast<ConnectionType>();

            // 创建 ConnectionMode 列表并填充
            List<ConnectionMode> connectionModes = connectionTypes.Select(type => new ConnectionMode
            {
                Mode = type,
                Description = GetEnumDescription(type)
            }).ToList();

            //选择连接模式
            var dialog = provider.CreateSelectionDialog(connectionModes, null, s =>
            {
                // 标题文本
                s.Title = "选择连接模式";
                // 消息文本
                s.Content = "不同的连接模式对应不同的权限功能，请根据需求选择";
                // 设置预检查回调
                s.PrecheckResult = item => new FormatCheckResult(item != null, "至少要选一项哦！");

                s.ItemDisplayTextConvertCallback = item => $"{item.Mode} ({item.Description})";
            });
            dialog.Show();
            if (dialog.Result.IsCancel)
            {
                Toast.Show("取消选择");
            }
            else
            {
                ConnMode = dialog.Result.Data;
                Toast.Show($"选中模式:{dialog.Result.Data.Description}");
                try
                {
                    //获取ip地址
                    var strs = textBox1.Text.Split(':');
                    TargetIP = strs[0];
                    TargetPort = Convert.ToInt32(strs[1]);

                    var client = new Client();
                    //初始化client
                    client.client.OnConnectedSuccessfully += (s, x) => Client_OnConnectedSuccessfully(client, x);
                    client.client.OnConnectionFailed += (s, x) => Client_OnConnectionFailed(client, x);
                    client.client.OnDisconnected += (s, x) => Client_OnDisconnected(client, x);
                    client.server.ClientConnected += (s, x) => Server_ClientConnected(client, x);
                    client.server.Started += (s, x) => Server_Started(client, x);
                    client.server.Closed += (s, x) => Server_Closed(client, x);
                    //连接
                    client.Connct(TargetIP, TargetPort);
                }
                catch (Exception ex)
                {
                    new ToastContentBuilder()
                     .AddText("DeskReach通知") // 标题文本
                     .AddText("请确保连接地址正确\n例:192.168.1.1:4555")
                     .Show(); // 7.0以上才提供Show方法
                    return;
                }
            }
        }

        /// <summary>
        /// 循环获取枚举注解
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private string GetEnumDescription(ConnectionType value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
            return attribute != null ? attribute.Description : value.ToString();
        }

        /// <summary>
        /// 将ip地址复制到剪切板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(comboBox.SelectedItem.ToString() + ":" + LocalPort);
        }

        /// <summary>
        /// 启动受控端服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            var client = new Client();
            //初始化client
            client.client.OnConnectedSuccessfully += (s, x) => Client_OnConnectedSuccessfully(client, x);
            client.client.OnConnectionFailed += (s, x) => Client_OnConnectionFailed(client, x);
            client.client.OnDisconnected += (s, x) => Client_OnDisconnected(client, x);
            client.server.ClientConnected += (s, x) => Server_ClientConnected(client, x);
            client.server.Started += (s, x) => Server_Started(client, x);
            client.server.Closed += (s, x) => Server_Closed(client, x);
            client.StartServer(comboBox.SelectedItem.ToString(), LocalPort);
        }

        private void MainFrom_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        public enum ConnectionType
        {
            /// <summary>
            /// 控制模式
            /// </summary>
            [Description("控制模式")]
            Control,

            /// <summary>
            /// 观看模式
            /// </summary>
            [Description("观看模式")]
            View,

            /// <summary>
            /// 命令行模式
            /// </summary>
            [Description("命令行模式")]
            Cmd,

            /// <summary>
            /// 文件传输模式
            /// </summary>
            [Description("文件传输模式")]
            FileTransfer
        }

        public class ConnectionMode
        {
            public ConnectionType Mode { get; set; }

            public string Description { get; set; }
        }
    }
}
