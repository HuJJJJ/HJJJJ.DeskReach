using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class ConvertExtension
    {
        public static int ToInt(this IEnumerable<byte> bytes, int offset = 0)
        {
            int value;
            value = (int)((bytes.ElementAt(offset) & 0xFF)
                    | ((bytes.ElementAt(offset + 1) & 0xFF) << 8)
                    | ((bytes.ElementAt(offset + 2) & 0xFF) << 16)
                    | ((bytes.ElementAt(offset + 3) & 0xFF) << 24));
            return value;
        }

        public static byte[] ToBytes(this int value)
        {
            byte[] src = new byte[4];
            src[3] = (byte)((value >> 24) & 0xFF);
            src[2] = (byte)((value >> 16) & 0xFF);
            src[1] = (byte)((value >> 8) & 0xFF);
            src[0] = (byte)(value & 0xFF);
            return src;
        }
    }
}
