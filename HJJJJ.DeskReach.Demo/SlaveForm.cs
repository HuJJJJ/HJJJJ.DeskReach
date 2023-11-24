using HJJJJ.DeskReach.Plugins.CommandPrompt.Windows;
using HJJJJ.DeskReach.Plugins.Keyboard;
using HJJJJ.DeskReach.Plugins.Pointer;
using HJJJJ.DeskReach.Plugins.Screen;
using HJJJJ.DeskReach.Plugins.TextMessage.Windows;
using HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HJJJJ.DeskReach.Demo
{
    public partial class SlaveForm : Form
    {
        private Client client;
        private PointerPlugin pointer;
        private ScreenPlugin screen;
        private TextMessagePlugin textMessage;
        private KeyboardPlugin keyboard;
        private DrawingBoardPlugin drawingBoard;
        private CommandPromptPlugin cmd;
        public SlaveForm(Client _client)
        {
            InitializeComponent();
            this.client = _client;
            var form = new MasterForm(client);
            cmd = new CommandPromptPlugin(new CMDForm(client));
            pointer = new PointerPlugin(form);
            screen = new ScreenPlugin(form);
            textMessage = new TextMessagePlugin(form);
            keyboard = new KeyboardPlugin(form);
            drawingBoard = new DrawingBoardPlugin(new DrawForm());
            client.RegPlugin(pointer);
            client.RegPlugin(screen);
            client.RegPlugin(textMessage);
            client.RegPlugin(keyboard);
            client.RegPlugin(drawingBoard);
            client.RegPlugin(cmd);
        }

        public void ShowCmdOutput(string data)
        {

        }
    }
}
