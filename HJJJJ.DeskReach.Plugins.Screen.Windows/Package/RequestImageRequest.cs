using System;
using System.Collections.Generic;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Screen.Windows.Package
{
    public class RequestImageRequest : ScreenPacket
    {

        public RequestImageRequest()
        {
            Code = ScreenActionType.RequestImageRequest;
        }

        public override byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Code);
            return bytes.ToArray();
        }
    }
}
