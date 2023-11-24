using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

public class WindowsAPIScreenCapture
{
    //这里是调用 Windows API函数来进行截图
    //首先导入库文件
    [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]

    //声明函数
    private static extern IntPtr CreateDC
     (
         string Driver,   //驱动名称
         string Device,   //设备名称
         string Output,   //无用，可以设定为null
         IntPtr PrintData //任意的打印机数据
      );


    [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
    private static extern bool BitBlt(
        IntPtr hdcDest,     //目标设备的句柄
        int XDest,          //目标对象的左上角X坐标
        int YDest,          //目标对象的左上角的Y坐标
        int Width,          //目标对象的宽度
        int Height,         //目标对象的高度
        IntPtr hdcScr,      //源设备的句柄
        int XScr,           //源设备的左上角X坐标
        int YScr,           //源设备的左上角Y坐标
        Int32 drRop         //光栅的操作值
        );
    [DllImport("gdi32.dll", EntryPoint = "GetDeviceCaps", SetLastError = true)]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
    private enum DeviceCap
    {
        VERTRES = 10,
        PHYSICALWIDTH = 110,
        SCALINGFACTORX = 114,
        DESKTOPVERTRES = 117,
    }

    /// <summary>
    /// 获取屏幕缩放比例
    /// </summary>
    /// <returns></returns>
    public static double GetScreenScalingFactor()
    {
        var g = Graphics.FromHwnd(IntPtr.Zero);
        IntPtr desktop = g.GetHdc();
        var physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
        var screenScalingFactor = (double)physicalScreenHeight / Screen.PrimaryScreen.Bounds.Height;
        //SystemParameters.PrimaryScreenHeight;

        return screenScalingFactor;
    }
    public static byte[] CaptureScreen(int quality)
    {
        //创建显示器的DC
        IntPtr dcScreen = CreateDC("DISPLAY", null, null, (IntPtr)null);

        //由一个指定设备的句柄创建一个新的Graphics对象
        Graphics g1 = Graphics.FromHdc(dcScreen);
        int tmpWidth, tmpHeigth;

        //tmpWidth = Screen.PrimaryScreen.Bounds.Width;
        //tmpHeigth = Screen.PrimaryScreen.Bounds.Height;
        var scalingFactor = GetScreenScalingFactor();
        tmpWidth = Convert.ToInt32(Screen.PrimaryScreen.Bounds.Width * scalingFactor);
        tmpHeigth = Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height * scalingFactor);
        System.Drawing.Image MyImage = new Bitmap(tmpWidth, tmpHeigth, g1);
        //创建位图图形对象
        Graphics g2 = Graphics.FromImage(MyImage);
        //获得窗体的上下文设备
        IntPtr dc1 = g1.GetHdc();
        //获得位图文件的上下文设备
        IntPtr dc2 = g2.GetHdc();
        //写入到位图
        BitBlt(dc2, 0, 0, tmpWidth, tmpHeigth, dc1, 0, 0, 13369376);
        //释放窗体的上下文设备
        g1.ReleaseHdc(dc1);
        //释放位图的上下文设备
        g2.ReleaseHdc(dc2);
        var image = MyImage.GetThumbnailImage(tmpWidth / 3, tmpHeigth / 3, null, IntPtr.Zero);
        MyImage.Dispose();
        return ZipImage(image, quality);
    }

    private static byte[] ZipImage(System.Drawing.Image image, long level)
    {

        ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
        EncoderParameters encoderParameters = new EncoderParameters(1);
        EncoderParameter encoderParameter = new EncoderParameter(Encoder.Quality, level);
        encoderParameters.Param[0] = encoderParameter;
        using (MemoryStream ms = new MemoryStream())
        {
            image.Save(ms, jpegEncoder, encoderParameters);
            var bytes = ms.ToArray();
            image.Dispose();
            return bytes;
        }

    }

    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {

        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

        foreach (ImageCodecInfo codec in codecs)
        {
            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }
        return null;
    }


}