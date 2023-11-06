using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Pointer
{

    public class PointerPacket : BasePacket
    {
        /// <summary>
        /// 鼠标X坐标
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// 鼠标Y坐标
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// 鼠标滚动
        /// </summary>
        public int Wheel { get; set; }

        /// <summary>
        /// 具体操作
        /// </summary>
        public PointerActionType Code { get; set; }

        /// <summary>
        /// 插件名称
        public override string PluginName { get; } = "HJJJJ.DeskReach.Plugins.Pointer.PointerPlugin";
        public PointerPacket(IEnumerable<byte> bytes) : base(bytes)
        {
            X = bytes.Take(4).ToInt();
            Y = bytes.Skip(4).Take(4).ToInt();
            Wheel = bytes.Skip(8).Take(4).ToInt();
            Code = (PointerActionType)bytes.Skip(12).Take(1).First();
        }

        public PointerPacket(Point location, PointerActionType type, int wheel = 0)
        {
            X = location.X;
            Y = location.Y;
            Wheel = wheel;
            Code = type;
        }

        public override byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(X.ToBytes());
            bytes.AddRange(Y.ToBytes());
            bytes.AddRange(Wheel.ToBytes());
            bytes.Add((byte)Code);
            return bytes.ToArray();
        }
    }
}
