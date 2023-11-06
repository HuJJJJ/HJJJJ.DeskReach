using HJJJJ.DeskReach.Plugins;
using HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows
{
    internal class DrawingBoardPacket : BasePacket
    {

        /// <summary>
        /// 划线
        /// </summary>
        public List<LineSegment> Lines { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public DrawingBoardActionType Code { get; set; }

        /// <summary>
        /// 插件名称
        /// </summary>
        public new string PluginName { get; } = "HJJJJ.DeskReach.Plugins.DrawingBoardPlugin";
        public DrawingBoardPacket(IEnumerable<byte> bytes) : base(bytes)
        {

        }

        public override byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Code);
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, Lines);
                bytes.AddRange(ms.ToArray());
            }
            return bytes.ToArray();
        }
    }
}
