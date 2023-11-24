using HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HJJJJ.DeskReach.Demo
{
    public partial class MainFrom : Form
    {

        private static string TargetIP { get; set; }
        private static int TargetPort { get; set; }

        private Client client = new Client();
        public MainFrom()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            textBox2.Text = "127.0.0.1:455";

            MasterForm masterForm = new MasterForm(client);
            FileTransferForm fileTransferForm = new FileTransferForm();
            SlaveForm slaveForm = new SlaveForm(client);
            fileTransferForm.FormClosed += SubForm_FormClosed;
            masterForm.FormClosed += SubForm_FormClosed;
            slaveForm.FormClosed += SubForm_FormClosed;
            //初始化组件并启动接收程序
            client.Server_OnConnectedSuccessfullyCallback = new Action(() =>
            {
                Invoke((MethodInvoker)delegate
                {
                    this.Hide();
                    slaveForm.Show();
                });

            });
            client.Client_OnConnectedSuccessfullyCallback = new Action(() =>
            {
                // 根据选择功能打开不同的窗体
                if (RemoteControlRadio.Checked)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        this.Hide();
                        masterForm.Show();
                    });
                }
                else if (FileTransferRadio.Checked)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        this.Hide();
                        fileTransferForm.Show();
                    });
                }
            });

        }

        /// <summary>
        /// 子窗体关闭后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubForm_FormClosed(object sender, FormClosedEventArgs e) => this.Show();

        private void button2_Click(object sender, EventArgs e)
        {

            try
            {
                //获取ip地址
                var strs = textBox1.Text.Split(':');
                TargetIP = strs[0];
                TargetPort = Convert.ToInt32(strs[1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("请确保连接地址正确\n例:192.168.1.1:4555");
                return;
            }
            //连接
            client.Connct(TargetIP, TargetPort);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(label3.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var strs = textBox2.Text.Split(':');
            client.StartServer(strs[0], Convert.ToInt32(strs[1]));
        }

        private void MainFrom_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }


    }
}
