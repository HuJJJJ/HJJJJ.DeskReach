using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Screen
{
    public class ScreenPacket : BasePacket


    {        /// <summary>
             /// 具体操作
             /// </summary>
        public ScreenActionType Code { get; set; }

        /// <summary>
        /// 修改图片质量
        /// </summary>
        public int ImageQuality { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        public byte[] Image { get; set; }


        /// <summary>
        /// 插件名称
        /// </summary>
        public override string PluginName { get; } = "HJJJJ.DeskReach.Plugins.Screen.ScreenPlugin";
        public ScreenPacket(IEnumerable<byte> bytes) : base(bytes)
        {
            Code = (ScreenActionType)bytes.Take(1).First();
            ImageQuality = bytes.Skip(1).Take(4).ToInt();
            Image = bytes.Skip(5).ToArray();
        }


        public ScreenPacket(ScreenActionType code, byte[] image = default, int imageQuality = 50)
        {
            Code = code;
            Image = image;
            ImageQuality = imageQuality;
        }

        public override byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Code);
            bytes.AddRange(ImageQuality.ToBytes());
            if (Image != null)
                bytes.AddRange(Image);
            return bytes.ToArray();
        }
    }
}
