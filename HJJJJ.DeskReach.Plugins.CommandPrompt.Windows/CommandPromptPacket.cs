using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HJJJJ.DeskReach.Plugins.CommandPrompt.Windows
{
    public class CommandPromptPacket : BasePacket
    {
        /// <summary>
        /// 输入命令或返回结果
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 功能码
        /// </summary>
        public CommandPromptActionType Code { get; set; }
        public override string PluginName { get; } = "HJJJJ.DeskReach.Plugins.CommandPrompt.Windows.CommandPromptPlugin";
        public CommandPromptPacket(byte[] bytes)
        {
            Code = (CommandPromptActionType)bytes.First();
            Data = Encoding.UTF8.GetString(bytes.Skip(1).ToArray());
        }

        public CommandPromptPacket(CommandPromptActionType code, string data="")
        {
            Code = code;
            Data = data;
        }


        public override byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Code);
            bytes.AddRange(Encoding.UTF8.GetBytes(Data));
            return bytes.ToArray();
        }
    }
}
