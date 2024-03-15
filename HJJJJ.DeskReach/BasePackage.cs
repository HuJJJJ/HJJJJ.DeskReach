using STTech.BytesIO.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HJJJJ.DeskReach
{
    public class BasePackage
    {

        public static readonly byte[] PackageHeadTag = new byte[] { 0xAA, 0xBB, 0xCC, 0xFF };

        /// <summary>
        /// 包长度
        /// </summary>
        public int PackageLen { get; set; }

        public int PluginNameLen { get; set; }

        /// <summary>
        /// 插件名称
        /// </summary>
        public string PluginName { get; set; }

        /// <summary>
        /// 细节数据
        /// </summary>
        public byte[] DetailData { get; set; }
        public BasePackage(byte[] bytes)
        {
            PackageLen = bytes.Skip(4).Take(4).ToInt();
            PluginNameLen = bytes.Skip(8).Take(4).ToInt();
            PluginName = Encoding.UTF8.GetString(bytes.Skip(12).Take(PluginNameLen).ToArray());
            DetailData = bytes.Skip(12 + PluginNameLen).ToArray();
        }

        public BasePackage(string pluginName, byte[] detailData)
        {
            PluginName = pluginName;
            DetailData = detailData;
            PluginNameLen = PluginName.Length;
            PackageLen =4+ 4 + 4 + PluginNameLen + DetailData.Length;
        }

        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(PackageHeadTag);
            bytes.AddRange(PackageLen.ToBytes());
            bytes.AddRange(PluginNameLen.ToBytes());
            bytes.AddRange(Encoding.UTF8.GetBytes(PluginName));
            bytes.AddRange(DetailData);
            return bytes.ToArray();
        }
    }
}
