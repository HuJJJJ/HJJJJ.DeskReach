using STTech.BytesIO.Core.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HJJJJ.DeskReach
{
    internal class PackageUnpacker : Unpacker
    {
        public PackageUnpacker()
        {

        }
        protected override int CalculatePacketLength(IEnumerable<byte> bytes)
        {
            return BitConverter.ToInt32(bytes.ToArray(), 0);
        }
    }
}
