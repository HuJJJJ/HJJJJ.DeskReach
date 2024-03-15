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
            StartMark = BasePackage.PackageHeadTag;
        }


        protected override int CalculatePacketLength(byte[] bytes)
        {
            try
            {
                var len = BitConverter.ToInt32(bytes, 4);
                if (len < 0)
                {
                    this.ClearCache();
                    return 0;
                }
                return len;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                this.ClearCache();
                return 0;
            }
           
        }
    }
}
