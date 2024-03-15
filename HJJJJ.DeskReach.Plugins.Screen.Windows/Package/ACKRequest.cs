using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HJJJJ.DeskReach.Plugins.Screen.Windows.Package
{
    public class ACKRequest:ScreenPacket
    {

        public ACKRequest()
        {
            Code = ScreenActionType.ACK;
        }

        public override byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Code);
            return bytes.ToArray();
        }
    }
}
