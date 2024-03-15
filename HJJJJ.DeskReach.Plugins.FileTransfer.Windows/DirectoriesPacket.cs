using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace HJJJJ.DeskReach.Plugins.FileTransfer.Windows
{
    public class DirectoriesPacket : BasePacket
    {
        public override string PluginName { get; } = "HJJJJ.DeskReach.Plugins.FileTransfer.Windows.FileTransferPlugin";
        /// <summary>
        /// 数据
        /// </summary>
        public List<TreeNode> TreeNodes { get; set; }

        public string Path { get; set; }

        public int PathLength { get; set; }

        public FunctionCode Code { get; set; }


        public DirectoriesPacket(FunctionCode code, List<TreeNode> data=null, string path = "")
        {
            Code =code;
            TreeNodes = data;
            Path = path;
            PathLength = path.Length;

        }

        public DirectoriesPacket(byte[] bytes)
        {
            Code = (FunctionCode)bytes[0];
            PathLength = BitConverter.ToInt32(bytes, 1);
            Path = Encoding.UTF8.GetString(bytes.Skip(5).Take(PathLength).ToArray());
            try
            {
                using (MemoryStream stream = new MemoryStream(bytes.Skip(5 + PathLength).ToArray()))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    TreeNodes = (List<TreeNode>)formatter.Deserialize(stream);
                }
            }
            catch (Exception)
            {
                TreeNodes = null;
            }
            
            }


        public override byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Code);
            bytes.AddRange(BitConverter.GetBytes(PathLength));
            bytes.AddRange(Encoding.UTF8.GetBytes(Path));
            if (TreeNodes != null) 
            {
                byte[] byteArray;

                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, TreeNodes);
                    byteArray = stream.ToArray();
                }
                bytes.AddRange(byteArray);
            }
            return bytes.ToArray();
        }


    }
}

