using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace HJJJJ.DeskReach.Plugins.FileTransfer.Windows
{
    public class FileTransferPacket : BasePacket
    {
        public override string PluginName { get; } = "HJJJJ.DeskReach.Plugins.FileTransfer.Windows.FileTransferPlugin";
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; set; }

        public string FileName { get; set; }

        public int DataLength { get; set; }

        public FunctionCode Code { get; set; }

        public string SessionId { get; set; }

        public int SessionIdLength { get; set; }

        public FileTransferPacket(FunctionCode code, byte[] data, string fileName = "", string sessionId = "")
        {
            Code = code;
            Data = data;
            DataLength = data.Length;
            FileName = fileName;
            SessionId = sessionId;
            SessionIdLength = SessionId.Length;
        }

        public FileTransferPacket(byte[] bytes)
        {
            Code = (FunctionCode)bytes[0];
            DataLength = BitConverter.ToInt32(bytes, 1);
            SessionIdLength = BitConverter.ToInt32(bytes, 5);
            Data = bytes.Skip(9).Take(DataLength).ToArray();
            SessionId = Encoding.UTF8.GetString(bytes.Skip(9+ DataLength).Take(SessionIdLength).ToArray());
            FileName = Encoding.UTF8.GetString(bytes.Skip(9 +SessionIdLength+ DataLength).ToArray());
        }


        public override byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Code);
            bytes.AddRange(BitConverter.GetBytes(DataLength));
            bytes.AddRange(BitConverter.GetBytes(SessionIdLength));
            bytes.AddRange(Data);
            bytes.AddRange(Encoding.UTF8.GetBytes(SessionId));
            bytes.AddRange(Encoding.UTF8.GetBytes(FileName));
            return bytes.ToArray();
        }


    }

    public enum FunctionCode
    {


        /// <summary>
        /// 正常传输文件
        /// </summary>
        Transfer,
        
        /// <summary>
        /// 获取计算机上的逻辑驱动器
        /// </summary>
        GetLogicalDrivesReuqest,
        GetLogicalDrivesResponse,

        /// <summary>
        /// 获取目录的子目录
        /// </summary>
        GetDirectoriesRequest,
        GetDirectoriesResponse,

        /// <summary>
        /// 传输完成重命名文件并移动文件
        /// </summary>
        RenameFiles,

        /// <summary>
        /// 设置文件保存的目录
        /// </summary>
        SetSaveDirectory
    }

}
