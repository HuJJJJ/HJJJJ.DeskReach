using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HJJJJ.DeskReach.Plugins.CommandPrompt.Windows
{
    public enum CommandPromptActionType:byte
    {
        /// <summary>
        /// 打开命令行
        /// </summary>
        Open,

        /// <summary>
        /// 输入命令
        /// </summary>
        Input,

        /// <summary>
        /// 获得返回结果
        /// </summary>
        OutPut,

        /// <summary>
        /// 关闭命令行窗口
        /// </summary>
        End
    }
}
