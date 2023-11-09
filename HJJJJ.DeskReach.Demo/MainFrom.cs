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
        RemoteControlFrom remoteControlFrom;
        public MainFrom()
        {
            InitializeComponent();
            //初始化组件并启动接收程序
            remoteControlFrom = new RemoteControlFrom();
            Store.BuidingPlugin(remoteControlFrom, remoteControlFrom, remoteControlFrom);
            textBox2.Text = "127.0.0.1:455";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                //获取ip地址
                var strs = textBox1.Text.Split(':');
                Store.TargetIP = strs[0];
                Store.TargetPort = Convert.ToInt32(strs[1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("请确保连接地址正确\n例:192.168.1.1:4555");
                return;
            }


            //根据选择功能打开不同的窗体
            if (RemoteControlRadio.Checked)
            {
                Store.ClientConnect();
                remoteControlFrom.ShowDialog();
            }
            else if (FileTransferRadio.Checked)
            {
                new FileTransferFrom().ShowDialog();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(label3.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var strs = textBox2.Text.Split(':');
            Store.ServerConnect(strs[0], Convert.ToInt32(strs[1]));
        }

        private void MainFrom_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }


    }
}
