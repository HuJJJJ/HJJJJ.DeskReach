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

        public MainFrom()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {     //获取ip地址
            var strs = textBox1.Text.Split(':');
            Store.TargetIP = strs[0];
            Store.TargetPort = Convert.ToInt32(strs[1]);

            //根据选择功能打开不同的窗体
            if (RemoteControlRadio.Checked)
            {
                new RemoteControlFrom().ShowDialog();
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
    }
}
