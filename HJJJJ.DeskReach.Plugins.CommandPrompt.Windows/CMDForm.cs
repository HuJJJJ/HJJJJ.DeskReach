using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HJJJJ.DeskReach.Plugins.CommandPrompt.Windows
{
    public partial class CMDForm : Form, ICommandPromptViewContext
    {
        private Client client;
        private CommandPromptPlugin cmd;
        public CMDForm(Client _client)
        {
            InitializeComponent();
            ///初始化客户端，初始化插件
            client = _client;
            cmd = new CommandPromptPlugin(this);
            client.RegPlugin(cmd);
        }

        public void HideCmd()
        {
            this.Hide();
        }

        public void ShowCmd()
        {
            this.Show();
        }

        public void ShowCmdOutput(string data)
        {
            richTextBox.AppendText(data);
        }

        private void richTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var str = richTextBox.Lines[richTextBox.Lines.Length - 1];
                var tempArr = str.Split('>');
                var command = "";
                if (tempArr.Length==1)
                    command = tempArr[0];
                else if (tempArr.Length==2)
                    command = tempArr[1];
                if (command == "") command = str;
                cmd.Action(new CommandPromptPacket(CommandPromptActionType.Input, command));
            }
        }

        private void CMDForm_Load(object sender, EventArgs e)
        {
            cmd.Action(new CommandPromptPacket(CommandPromptActionType.Open));
        }

        private void CMDForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            return;
        }
    }
}
