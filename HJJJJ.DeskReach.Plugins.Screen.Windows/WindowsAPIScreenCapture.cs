using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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

    public int Width = 0;
    public int Height = 0;
    public Bitmap ResultThumbnail;
    private Graphics g;
    private IntPtr dcScreen;
    private Graphics g1;
    Graphics g2;
    Bitmap OriginalImage;
    public WindowsAPIScreenCapture()
    {
        var scalingFactor = GetScreenScalingFactor();
        Width = Convert.ToInt32(Screen.PrimaryScreen.Bounds.Width * scalingFactor);
        Height = Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height * scalingFactor);
    }

    public void Init(Bitmap bitmap)
    {
        ResultThumbnail = bitmap;
        g = Graphics.FromImage(ResultThumbnail);
        dcScreen = CreateDC("DISPLAY", null, null, (IntPtr)null);
        g1 = Graphics.FromHdc(dcScreen);
        OriginalImage = new Bitmap(Width, Height, g1);
        g2 = Graphics.FromImage(OriginalImage);
    }

  static  object obj = new object();
    public void CaptureScreen()
    {
        try
        {
            lock (obj)
            {
                IntPtr dc1 = g1.GetHdc();
                IntPtr dc2 = g2.GetHdc();
                BitBlt(dc2, 0, 0, Width, Height, dc1, 0, 0, 13369376);
                g1.ReleaseHdc(dc1);
                g2.ReleaseHdc(dc2);
                g.DrawImage(OriginalImage, new Rectangle(0, 0, Width / 2, Height / 2));
            }
        
        }
        catch (Exception)
        {

        }
    
    }

    public void Dispose()
    {
        g1.Dispose();
        ResultThumbnail.Dispose();
        g.Dispose();
        g2.Dispose();
        OriginalImage.Dispose();
    }


    /// <summary>
    /// 获取屏幕缩放比例
    /// </summary>
    /// <returns></returns>
    public  double GetScreenScalingFactor()
    {
        var g = Graphics.FromHwnd(IntPtr.Zero);
        IntPtr desktop = g.GetHdc();
        var physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
        var screenScalingFactor = (double)physicalScreenHeight / Screen.PrimaryScreen.Bounds.Height;
        return screenScalingFactor;
    }



}