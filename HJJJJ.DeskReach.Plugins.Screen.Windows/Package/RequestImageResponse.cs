using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Windows.Forms.AxHost;

namespace HJJJJ.DeskReach.Plugins.Screen.Windows.Package
{
    public class RequestImageResponse : ScreenPacket
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public RequestImageResponse(int width, int height)
        {
            Code = ScreenActionType.RequestImageResponse;
            Width = width;
            Height = height;
        }

        public RequestImageResponse(byte[] bytes) : base(bytes)
        {
            Code = (ScreenActionType)bytes.First();
            Width = BitConverter.ToInt32(bytes.Skip(1).Take(4).ToArray(), 0);
            Height = BitConverter.ToInt32(bytes.Skip(5).Take(4).ToArray(), 0);
        }


        public override byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Code);
            bytes.AddRange(BitConverter.GetBytes(Width));
            bytes.AddRange(BitConverter.GetBytes(Height));
            return bytes.ToArray();
        }
    }
}
