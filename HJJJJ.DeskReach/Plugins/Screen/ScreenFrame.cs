using System;
using System.Collections.Generic;
using System.IO;

namespace HJJJJ.DeskReach.Plugins.Screen
{
    /// <summary>版本 2：一张关键帧，或基于指定上一帧的若干独立 JPEG 区域。</summary>
    public sealed class ScreenFrame
    {
        public const int MaxFrameBytes = 32 * 1024 * 1024;
        public const int MaxPixels = 16 * 1024 * 1024;
        public const int HeaderSize = 52;
        private const int Magic = 0x32535244; // DRS2
        public Guid StreamId { get; set; }
        public long FrameId { get; set; }
        public long BaseFrameId { get; set; }
        public bool IsKeyFrame { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<ScreenRegion> Regions { get; } = new List<ScreenRegion>();

        public static ScreenFrame ReadHeader(byte[] bytes)
        {
            if (bytes == null || bytes.Length < HeaderSize || bytes.Length > MaxFrameBytes)
                throw new InvalidDataException("屏幕帧长度无效。");
            using (var stream = new MemoryStream(bytes, false))
            using (var reader = new BinaryReader(stream)) return ReadHeader(reader);
        }

        private static ScreenFrame ReadHeader(BinaryReader reader)
        {
            if (reader.ReadInt32() != Magic || reader.ReadByte() != 2)
                throw new InvalidDataException("屏幕协议版本不匹配，请更新两端程序。");
            byte flags = reader.ReadByte();
            if (flags > 1 || reader.ReadUInt16() != 0) throw new InvalidDataException("屏幕帧标志无效。");
            var frame = new ScreenFrame { IsKeyFrame = flags == 1, Width = reader.ReadInt32(), Height = reader.ReadInt32(),
                StreamId = new Guid(reader.ReadBytes(16)), FrameId = reader.ReadInt64(), BaseFrameId = reader.ReadInt64() };
            frame.ValidateHeader();
            return frame;
        }

        private void ValidateHeader()
        {
            if (Width <= 0 || Height <= 0 || Width > 8192 || Height > 8192 || (long)Width * Height > MaxPixels ||
                StreamId == Guid.Empty || FrameId <= 0 || (IsKeyFrame ? BaseFrameId != 0 : BaseFrameId <= 0 || BaseFrameId >= FrameId))
                throw new InvalidDataException("屏幕帧尺寸或序号无效。");
        }

        private void ValidateRegion(ScreenRegion region)
        {
            if (region.Width <= 0 || region.Height <= 0 || region.Width > Width || region.Height > Height ||
                region.X < 0 || region.Y < 0 || region.X > Width - region.Width || region.Y > Height - region.Height ||
                region.Jpeg == null || region.Jpeg.Length == 0)
                throw new InvalidDataException("屏幕更新区域无效。");
        }

        public byte[] GetBytes()
        {
            ValidateHeader();
            ValidateRegions();
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Magic); writer.Write((byte)2); writer.Write((byte)(IsKeyFrame ? 1 : 0)); writer.Write((ushort)0);
                writer.Write(Width); writer.Write(Height); writer.Write(StreamId.ToByteArray());
                writer.Write(FrameId); writer.Write(BaseFrameId); writer.Write(Regions.Count);
                foreach (var region in Regions)
                {
                    if (stream.Length + 20L + region.Jpeg.Length > MaxFrameBytes) throw new InvalidDataException("屏幕帧过大。");
                    writer.Write(region.X); writer.Write(region.Y); writer.Write(region.Width); writer.Write(region.Height);
                    writer.Write(region.Jpeg.Length); writer.Write(region.Jpeg);
                }
                return stream.ToArray();
            }
        }

        private void ValidateRegions()
        {
            if (Regions.Count == 0 || Regions.Count > 4096) throw new InvalidDataException("区域数量无效。");
            foreach (var region in Regions) ValidateRegion(region);
            if (IsKeyFrame && (Regions.Count != 1 || Regions[0].X != 0 || Regions[0].Y != 0 ||
                Regions[0].Width != Width || Regions[0].Height != Height)) throw new InvalidDataException("关键帧必须覆盖全图。");
        }

        public static ScreenFrame Parse(byte[] bytes)
        {
            ReadHeader(bytes);
            try
            {
                using (var stream = new MemoryStream(bytes, false))
                using (var reader = new BinaryReader(stream))
                {
                    var frame = ReadHeader(reader);
                    int count = reader.ReadInt32();
                    if (count <= 0 || count > 4096 || count > (stream.Length - stream.Position) / 21)
                        throw new InvalidDataException("区域数量无效。");
                    for (int i = 0; i < count; i++)
                    {
                        var region = new ScreenRegion { X = reader.ReadInt32(), Y = reader.ReadInt32(),
                            Width = reader.ReadInt32(), Height = reader.ReadInt32() };
                        int length = reader.ReadInt32();
                        if (length <= 0 || length > stream.Length - stream.Position) throw new InvalidDataException("JPEG 长度无效。");
                        region.Jpeg = reader.ReadBytes(length);
                        frame.Regions.Add(region);
                    }
                    if (stream.Position != stream.Length) throw new InvalidDataException("屏幕帧含多余数据。");
                    frame.ValidateRegions();
                    return frame;
                }
            }
            catch (EndOfStreamException ex) { throw new InvalidDataException("屏幕帧不完整。", ex); }
        }

        public byte[] GetAcknowledgement()
        {
            var bytes = new byte[24];
            Buffer.BlockCopy(StreamId.ToByteArray(), 0, bytes, 0, 16);
            Buffer.BlockCopy(BitConverter.GetBytes(FrameId), 0, bytes, 16, 8);
            return bytes;
        }
    }

    public sealed class ScreenRegion
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Jpeg { get; set; }
    }
}
