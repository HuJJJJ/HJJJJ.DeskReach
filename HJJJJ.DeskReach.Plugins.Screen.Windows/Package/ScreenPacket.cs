using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Screen.Windows
{
    public class ScreenPacket : BasePacket


    {        /// <summary>
             /// 具体操作
             /// </summary>
        public ScreenActionType Code { get; set; }



        /// <summary>
        /// 插件名称
        /// </summary>
        public override string PluginName { get; } = "HJJJJ.DeskReach.Plugins.Screen.Windows.ScreenPlugin";
        public ScreenPacket(byte[] bytes) : base(bytes)
        {
        }
        public ScreenPacket()
        {
            
        }
        public override byte[] GetBytes()
        {
            return null;
        }
    }
}
