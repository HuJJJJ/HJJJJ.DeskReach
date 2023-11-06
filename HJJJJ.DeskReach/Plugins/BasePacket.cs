using STTech.BytesIO.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace HJJJJ.DeskReach.Plugins
{

    ////////////////////////////
    //////////协议格式////////  ///四位X坐标///四位Y坐标///4位Wheel///1位Code////
    ////////////////////////////
    public abstract class BasePacket
    {

        public virtual string PluginName { get; } = "";

        public BasePacket(IEnumerable<byte> bytes)
        {
        }

        public BasePacket()
        {

        }

        public abstract byte[] GetBytes();
    }
}
