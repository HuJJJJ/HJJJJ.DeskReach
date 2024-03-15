using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace HJJJJ.DeskReach.Plugins.Screen.Windows
{
    public class BitmapMemoryIO
    {

        public static byte[] ReadBitmapBlock(Bitmap bitmap, Rectangle rectangle)
        {
            var bmpData = bitmap.LockBits(rectangle, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            // 计算矩形区域的像素数据长度
            int regionByteCount = rectangle.Width * rectangle.Height * bytesPerPixel;

            // 从位图像素数据中提取特定区域的像素数据
            byte[] regionPixels = new byte[regionByteCount];
            Marshal.Copy(bmpData.Scan0, regionPixels, 0, regionByteCount);
            bitmap.UnlockBits(bmpData);
            return regionPixels;
        }

        public static void WriteBitmapBlock(Bitmap bitmap, Rectangle rectangle, byte[] data)
        {
            var bmpData = bitmap.LockBits(rectangle, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
            bitmap.UnlockBits(bmpData);
        }

        public static bool CompareDifference(Bitmap originalBitmap, Bitmap bitmap, Rectangle rectangle)
        {
            var originalBitmapData = originalBitmap.LockBits(rectangle, ImageLockMode.ReadWrite, originalBitmap.PixelFormat);
            var bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(originalBitmapData.PixelFormat) / 8; // 每个像素的字节数
            int stride = originalBitmapData.Stride; // 每行的字节数

            if (originalBitmapData.PixelFormat != bitmapData.PixelFormat) return false;
            unsafe
            {
                byte* originalPtr = (byte*)originalBitmapData.Scan0;
                byte* ptr = (byte*)bitmapData.Scan0;

                for (int y = 0; y < rectangle.Height; y++)
                {
                    for (int x = 0; x < rectangle.Width; x++)
                    {
                        int offset = y * stride + x * bytesPerPixel;
                        byte oldB = originalPtr[offset];
                        byte oldG = originalPtr[offset + 1];
                        byte oldR = originalPtr[offset + 2];
                        byte newB = ptr[offset];
                        byte newG = ptr[offset + 1];
                        byte newR = ptr[offset + 2];

                        if (oldB != newB || oldG != newG || oldR != newR)
                        {
                            originalBitmap.UnlockBits(originalBitmapData);
                            bitmap.UnlockBits(bitmapData);
                            return false;
                        }

                        //if (originalPtr[y * stride + x] != ptr[y * stride + x])
                        //{
                        //    originalBitmap.UnlockBits(originalBitmapData);
                        //    bitmap.UnlockBits(bitmapData);
                        //    return false; 
                        //}
                    }
                }
            }

            originalBitmap.UnlockBits(originalBitmapData);
            bitmap.UnlockBits(bitmapData);
            return true;
        }

        public static bool CompareDifference(Bitmap bitmap, Rectangle rectangle, byte[] data)
        {
            var bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadWrite, bitmap.PixelFormat);

            int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8; // 每个像素的字节数
            int stride = bitmapData.Stride; // 每行的字节数

            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;
                int offset = 0;
                for (int y = 0; y < rectangle.Height; y++)
                {
                    for (int x = 0; x < rectangle.Width; x++)
                    {
                        offset = y * stride + x;
                        if (offset >= data.Length) break;
                        if (ptr[offset] != data[offset])
                        {
                            bitmap.UnlockBits(bitmapData);
                            return false;
                        }
                    }
                    if (offset >= data.Length) break;
                }
            }

            bitmap.UnlockBits(bitmapData);
            return true;
        }


    }
}
