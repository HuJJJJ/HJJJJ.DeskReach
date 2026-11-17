using HJJJJ.DeskReach.Plugins;
using HJJJJ.DeskReach.Plugins.CommandPrompt.Windows;
using HJJJJ.DeskReach.Plugins.Keyboard;
using HJJJJ.DeskReach.Plugins.Pointer;
using HJJJJ.DeskReach.Plugins.Screen;
using HJJJJ.DeskReach.Plugins.TextMessage.Windows;
using HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows;
using System;
using System.Windows.Forms;

namespace HJJJJ.DeskReach.Windows
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
            // 此窗体只是受控端插件的隐藏视图，也需要随会话一起释放。
            Disposed += (sender, args) => form.Dispose();
            cmd = new CommandPromptPlugin(new CMDForm(client, null));
            pointer = new PointerPlugin(form);
            screen = new ScreenPlugin(form);
            textMessage = new TextMessagePlugin(form);
            keyboard = new KeyboardPlugin(form);
            drawingBoard = new DrawingBoardPlugin(new DrawForm(null));
            //设置checkbox的tag
            pointerCheckBox.Tag = pointer;
            keyBoardCheckBox.Tag = keyboard;
            drawingBoardCheckBox.Tag = drawingBoard;
            cmdCheckBox.Tag = cmd;
            //注册插件
            client.RegPlugin(pointer);
            client.RegPlugin(screen);
            client.RegPlugin(textMessage);
            client.RegPlugin(keyboard);
            client.RegPlugin(drawingBoard);
            client.RegPlugin(cmd);
        }

        private void CheckBox_ChckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            BasePlugin plugin = (BasePlugin)checkBox.Tag; // 假设每个 CheckBox 的 Tag 属性保存了对应的插件对象
            if (checkBox.Checked)
            {
                client.RegPlugin(plugin);
            }
            else
            {
                client.UnregPlugin(plugin);
            }
        }
    }
}
