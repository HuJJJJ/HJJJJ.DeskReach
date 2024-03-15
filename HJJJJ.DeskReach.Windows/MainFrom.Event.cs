using ApeFree.ApeForms.Forms.Notifications;
using HJJJJ.DeskReach.Entities;
using HJJJJ.DeskReach.Plugins.CommandPrompt.Windows;
using Microsoft.Toolkit.Uwp.Notifications;
using STTech.BytesIO.Core.Component;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HJJJJ.DeskReach.Windows
{
    public partial class MainFrom
    {

        /// <summary>
        /// 连接失败时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_OnConnectionFailed(object sender, STTech.BytesIO.Core.ConnectionFailedEventArgs e)
        {
            new ToastContentBuilder()
.AddText("DeskReach发来一条消息") // 标题文本
.AddText("-----------------连接错误-----------------")
.AddText("错误码: " + e.ErrorCode)
.AddText("错误信息:" + e.Message)
.AddText("错误类型:" + e.Exception.GetType().ToString())
.Show(); // 7.0以上才提供Show方法
        }



        /// <summary>
        /// 客户端连接事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_OnConnectedSuccessfully(object sender, STTech.BytesIO.Core.ConnectedSuccessfullyEventArgs e)
        {
            var client = sender as Client;
            Invoke((MethodInvoker)delegate
            {
                // this.Hide();
                switch (ConnMode.Mode)
                {
                    case ConnectionType.Control:
                        var masterForm = new MasterForm(client, ConnMode.Mode);
                        masterForm.FormClosed += (s, x) => SubForm_FormClosed(client, x);
                        AddFormToPage(masterForm, client.client.Host);
                        break;
                    case ConnectionType.View:
                        var masterForm1 = new MasterForm(client, ConnMode.Mode);
                        masterForm1.FormClosed += (s, x) => SubForm_FormClosed(client, x);
                        AddFormToPage(masterForm1, client.client.Host);
                        break;
                    case ConnectionType.Cmd:
                        //var cmdControlForm = new CMDControlForm(client, ConnectionType.Cmd);
                        //cmdControlForm.FormClosed += (s, x) => SubForm_FormClosed(client, x);

                        var cmdForm = new CMDForm(client, this);
                        cmdForm.FormClosed += (s, x) => SubForm_FormClosed(client, x);
                        var command = new CommandPromptPlugin(cmdForm);
                        command.OpenCmd();
                        AddFormToPage(cmdForm, client.client.Host);
                        break;
                    case ConnectionType.FileTransfer:
                        var fileTransferForm = new FileTransferForm(client, ConnMode.Mode);
                        fileTransferForm.FormClosed += (s, x) => SubForm_FormClosed(client, x);
                        AddFormToPage(fileTransferForm, client.client.Host);
                        break;
                    default:
                        break;
                }

            });
            client.CloseServer();
        }

        private void AddFormToPage(Form form, string host)
        {
            form.TopLevel = false;
            ClosableTabPage tb = new ClosableTabPage(host, form.Name, () => { form.Close(); this.closableTabControl1.TabPages.Remove(this.closableTabControl1.SelectedTab); });
            tb.Controls.Add(form); //将窗体添加到form中
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Show();
            this.closableTabControl1.TabPages.Add(tb);
            closableTabControl1.SelectedTab = tb;
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_OnDisconnected(object sender, STTech.BytesIO.Core.DisconnectedEventArgs e)
        {
            new ToastContentBuilder()
.AddText("DeskReach发来一条消息") // 标题文本
.AddText("已取消控制")
.Show(); // 7.0以上才提供Show方法
        }



        /// <summary>
        /// sd\\服务器启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_Started(object sender, EventArgs e)
        {
            new ToastContentBuilder()
.AddText("DeskReach发来一条消息") // 标题文本
.AddText("已准备完毕，可接受控制")
.Show(); // 7.0以上才提供Show方法

        }

        /// <summary>
        /// 服务端连接事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_ClientConnected(object sender, STTech.BytesIO.Tcp.Entity.ClientConnectedEventArgs e)
        {
            var client = sender as Client;
            Invoke((MethodInvoker)delegate
            {
                this.Hide();
              var  slaveForm = new SlaveForm(client);
                slaveForm.FormClosed += (s, x) => SubForm_FormClosed(client, x);
                slaveForm.Show();
            });
            client.CloseClient();
        }

        /// <summary>
        /// 服务器关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_Closed(object sender, EventArgs e)
        {
            new ToastContentBuilder()
.AddText("DeskReach发来一条消息") // 标题文本
.AddText("关闭受控模式")
.Show(); // 7.0以上才提供Show方法
        }
    }
}
