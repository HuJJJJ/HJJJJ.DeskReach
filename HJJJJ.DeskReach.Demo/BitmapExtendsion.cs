using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing
{
internal static class BitmapExtendsion
{
    /// <summary>
    /// bitmap转bytes
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    public static byte[] ToBytes(this Bitmap bitmap)
    {
        // 1.先将BitMap转成内存流
        using (MemoryStream ms = new MemoryStream())
        {
            bitmap.Save(ms, ImageFormat.Bmp);
            return ms.ToArray();
        }
    }



    /// <summary>
    /// bytes转bitmap
    /// </summary>
    /// <param name="Bytes"></param>
    /// <returns></returns>
    public static Bitmap ToBitmap(this byte[] Bytes)
    {
        MemoryStream stream = null;
        try
        {
            stream = new MemoryStream(Bytes);
            return new Bitmap(stream);
        }
        catch (ArgumentNullException ex)
        {
            throw ex;
        }
        catch (ArgumentException ex)
        {
            throw ex;
        }
        finally
        {
            stream.Close();
        }
    }



    /// <summary>
    /// 等比例缩放图片
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="destHeight"></param>
    /// <param name="destWidth"></param>
    /// <returns></returns>
    public static Bitmap ZoomImage(this Bitmap bitmap, int destHeight, int destWidth)
    {
        try
        {
            System.Drawing.Image sourImage = bitmap;
            int width = 0, height = 0;
            //按比例缩放           
            int sourWidth = sourImage.Width;
            int sourHeight = sourImage.Height;
            if (sourHeight > destHeight || sourWidth > destWidth)
            {
                if ((sourWidth * destHeight) > (sourHeight * destWidth))
                {
                    width = destWidth;
                    height = (destWidth * sourHeight) / sourWidth;
                }
                else
                {
                    height = destHeight;
                    width = (sourWidth * destHeight) / sourHeight;
                }
            }
            else
            {
                width = sourWidth;
                height = sourHeight;
            }
            Bitmap destBitmap = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage(destBitmap);
            g.Clear(Color.Transparent);
            //设置画布的描绘质量         
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
            g.DrawImage(sourImage, new Rectangle((destWidth - width) / 2, (destHeight - height) / 2, width, height), 0, 0, sourImage.Width, sourImage.Height, GraphicsUnit.Pixel);
            g.Dispose();
            //设置压缩质量     
            EncoderParameters encoderParams = new EncoderParameters();
            long[] quality = new long[1];
            quality[0] = 100;
            EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            sourImage.Dispose();
            return destBitmap;
        }
        catch
        {
            return bitmap;
        }
    }

}

}

