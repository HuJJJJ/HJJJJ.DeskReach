using HJJJJ.DeskReach.Plugins.Pointer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HJJJJ.DeskReach.Plugins.TextMessage.Windows
{
    public class TextMessagePacket : BasePacket
    {

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 具体操作
        /// </summary>
        public TextMessageActionType Code { get; set; }

        /// <summary>
        /// 插件名称
        public override string PluginName { get; } = "HJJJJ.DeskReach.Plugins.TextMessage.Windows.TextMessagePlugin";
        public TextMessagePacket(IEnumerable<byte> bytes) : base(bytes)
        {

            Code = (TextMessageActionType)bytes.First();
            Message = Encoding.UTF8.GetString(bytes.Skip(1).ToArray());

        }

        public TextMessagePacket(string message, TextMessageActionType code)
        {
            Message = message;
            Code = code;
        }

        public override byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Code);
            bytes.AddRange(Encoding.UTF8.GetBytes(Message));
            return bytes.ToArray();
        }
    }
}
