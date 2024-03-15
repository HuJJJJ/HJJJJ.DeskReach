using HJJJJ.DeskReach.Plugins.CommandPrompt.Windows;
using HJJJJ.DeskReach.Plugins.PluginMenu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static HJJJJ.DeskReach.Windows.MainFrom;

namespace HJJJJ.DeskReach.Windows
{
    public partial class CMDControlForm : Form
    {
        private Client client;
        private CommandPromptPlugin command;
        private ConnectionType ConnectionMode;
        public CMDControlForm(Client _client, ConnectionType type ) 
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            ConnectionMode = type;
            //初始化客户端和插件
            this.client = _client;
            var cmdForm = new CMDForm(client, this);
            cmdForm.FormClosing += CmdForm_FormClosing;
            command = new CommandPromptPlugin(cmdForm);
            command.OpenCmd();
            this.FormClosing += (s,e)=> 
            {
            
            };
        }

        private void CmdForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Close();
        }


    }
}
