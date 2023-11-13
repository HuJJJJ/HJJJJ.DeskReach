using HJJJJ.DeskReach.Plugins;
using HJJJJ.DeskReach.Plugins.Keyboard;
using HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

namespace HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows
{
    public class DrawingBoardPacket : BasePacket
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
        public override string PluginName { get; } = "HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows.DrawingBoardPlugin";
        public DrawingBoardPacket(IEnumerable<byte> bytes) : base(bytes)
        {
            Code = (DrawingBoardActionType)bytes.Take(1).First();


            //反序列化list
            if (bytes.Count()>1)
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream(bytes.Skip(1).ToArray()))
                {
                    Lines = (List<LineSegment>)bf.Deserialize(ms);
                }
            }
         
        }

        public DrawingBoardPacket(DrawingBoardActionType code, List<LineSegment> lines = null)
        {
            Code = code;
            Lines = lines;
        }

        public override byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Code);


            if (Lines == null) return bytes.ToArray();

            //序列化list
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
