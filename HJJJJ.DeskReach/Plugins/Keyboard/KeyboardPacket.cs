using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Keyboard
{
    public class KeyboardPacket : BasePacket
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        public KeyboardActionType Code { get; set; }

        /// <summary>
        /// 键盘键位
        /// </summary>
        public string KeyChars { get; set; }

        /// <summary>
        /// 插件名称
        /// </summary>
        public override string PluginName { get; } = "HJJJJ.DeskReach.Plugins.Keyboard.KeyboardPlugin";


        public KeyboardPacket(IEnumerable<byte> bytes) : base(bytes)
        {
            Code = (KeyboardActionType)bytes.Take(1).First();
            KeyChars = Encoding.UTF8.GetString(bytes.Skip(1).ToArray());
        }

        public KeyboardPacket(KeyboardActionType code , string keyChars)
        {
            Code = code;
            KeyChars = keyChars;
        }

        public override byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Code);
            bytes.AddRange(Encoding.UTF8.GetBytes(KeyChars));
            return bytes.ToArray();
        }
    }
}
