using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HJJJJ.DeskReach.Plugins.CommandPrompt.Windows
{
    public interface ICommandPromptViewContext
    {


        /// <summary>
        /// 显示cmd窗口
        /// </summary>
        void ShowCmd();

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        void HideCmd();

        /// <summary>
        /// 显示命令行输出
        /// </summary>
         void ShowCmdOutput(string data);
    }
}
