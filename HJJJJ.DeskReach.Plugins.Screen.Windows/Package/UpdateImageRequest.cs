using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Screen.Windows.Package
{
    public class UpdateImageRequest : ScreenPacket
    {
        /// <summary>
        /// 图片
        /// </summary>
        public byte[] Image { get; set; }

        ///// <summary>
        ///// 扫描块大小
        ///// </summary>
        //public int BlockSize;

        /// <summary>
        /// 扫描块长度
        /// </summary>
        public int HeightBlockSize;

        /// <summary>
        /// 扫描块宽度
        /// </summary>
        public int WidthBlockSize;

        /// <summary>
        /// X坐标起始点
        /// </summary>
        public int StartX;

        /// <summary>
        /// Y坐标起始点
        /// </summary>
        public int StartY;

        /// <summary>
        /// 时间戳标识
        /// </summary>
        public long TimeStamp;




        public UpdateImageRequest(int startX, int startY, byte[] image, int heightBlockSize, int widthBlockSize,long timeStamp)
        {
            Code = ScreenActionType.UpdateImage;
            StartX = startX;
            StartY = startY;
            Image = image;
            HeightBlockSize = heightBlockSize;
            WidthBlockSize = widthBlockSize;
            TimeStamp = timeStamp; 
        }

        public UpdateImageRequest(byte[] bytes) : base(bytes)
        {
            Code = (ScreenActionType)bytes.First();
            StartX = BitConverter.ToInt32(bytes, 1);
            StartY = BitConverter.ToInt32(bytes, 5);
            WidthBlockSize = BitConverter.ToInt32(bytes, 9);
            HeightBlockSize = BitConverter.ToInt32(bytes, 13);
            TimeStamp = BitConverter.ToInt64(bytes, 17);
            Image = bytes.Skip(25).ToArray();
        }


        public override byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Code);
            bytes.AddRange(BitConverter.GetBytes(StartX));
            bytes.AddRange(BitConverter.GetBytes(StartY));
            bytes.AddRange(BitConverter.GetBytes(WidthBlockSize));
            bytes.AddRange(BitConverter.GetBytes(HeightBlockSize));
            bytes.AddRange(BitConverter.GetBytes(TimeStamp));
            bytes.AddRange(Image);
            return bytes.ToArray();
        }
    }
}
