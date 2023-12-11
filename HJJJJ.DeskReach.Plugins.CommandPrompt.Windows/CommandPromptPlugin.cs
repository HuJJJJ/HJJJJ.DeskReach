using HJJJJ.DeskReach.Plugins.PluginMenu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HJJJJ.DeskReach.Plugins.CommandPrompt.Windows
{
    public class CommandPromptPlugin : BasePlugin
    {
        private ICommandPromptViewContext ViewContext { get; set; }

        private Command command = new Command();
        public new event EventHandler<byte[]> OnDataReceived;
        public CommandPromptPlugin(ICommandPromptViewContext viewContext)
        {
            ViewContext = viewContext;
            this.OnDataReceived += CommandPromptPlugin_OnDataReceived; ;
        }


        /// <summary>
        /// 注册client
        /// </summary>
        /// <param name="client"></param>
        public override void RegInit(Client client)
        {
            base.RegInit(client);
        }


        public override List<PluginMenu.MenuItem> GetMenuItems(MenuPosition pos)
        {
            var menu = new List<PluginMenu.MenuItem>();
            switch (pos)
            {
                case MenuPosition.TopMenu:
                    break;
                case MenuPosition.Screen:
                    break;
                case MenuPosition.ToolBar:
                    break;
                case MenuPosition.ToolMenu:
                    break;
                case MenuPosition.StatusMenu:
                    menu.Add(new ButtonMenuItem()
                    {
                        Name = "打开命令行",
                        OnClick = new Action<PluginMenu.MenuItem, EventArgs>((sender, e) =>
                        {
                            ViewContext.ShowCmd();
                        })
                    });
                    break;
                   
            }
                    return menu;
        }
        private void CommandPromptPlugin_OnDataReceived(object sender, byte[] e)
        {
            var data = new CommandPromptPacket(e);
            switch (data.Code)
            {
                case CommandPromptActionType.Open:
                    command = new Command();
                    command.Output += (str) => Action(new CommandPromptPacket(CommandPromptActionType.OutPut, str)); ;
                    command.Error += (str) => Action(new CommandPromptPacket(CommandPromptActionType.OutPut, str)); ;
                    break;
                case CommandPromptActionType.Input:
                    command.RunCMD(data.Data);
                    break;
                case CommandPromptActionType.OutPut:
                    ViewContext.ShowCmdOutput(data.Data);
                    break;
                case CommandPromptActionType.End:
                    command.Stop();
                    ViewContext.HideCmd();
                    break;
            }
        }


        /// <summary>
        /// 接收消息处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void RaiseDataReceived(object sender, byte[] e) => SafelyInvokeCallback(() => { OnDataReceived?.Invoke(sender, e); });


        /// <summary>
        /// 文本操作
        /// </summary>
        public void Action(CommandPromptPacket packet) => Send(packet);
    }
}
