using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HJJJJ.DeskReach.Demo
{
    internal static class ScreenExtension
    {
        public static Rectangle GetScreenArea(this Form form)
        {
            return Screen.GetBounds(form);
        }

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


        /// <summary>
        /// 获取实际分辨率
        /// </summary>
        /// <param name="form"></param>
        public static Size GetActualResolution()
        {
            try
            {
                //获取缩放比例，获得物理的屏幕分辨率
                var scalingFactor = GetScreenScalingFactor();
                var width = (double)Screen.PrimaryScreen.Bounds.Width;
                var height = (double)Screen.PrimaryScreen.Bounds.Height;
                width = width * scalingFactor;
                height = height * scalingFactor;

                return new Size()
                {
                    Width = (int)width,
                    Height = (int)height
                };


            }
            catch (Exception ex)
            {
                //异常处理
                MessageBox.Show(ex.ToString());
                return new Size();
            }
        }


        /// <summary>
        /// 获取当前屏幕图片bitmap
        /// </summary>
        /// <param name="form"></param>?

        public static Bitmap GetTheCurrentScreenImage()
        {
            var size = GetActualResolution();
            //生成截图的位图容器  
            Bitmap bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);
            //rc.Size = bitmap.Size;
            //GDI+图像画布  
            using (Graphics memoryGrahics = Graphics.FromImage(bitmap))
            {
                memoryGrahics.CopyFromScreen(0, 0, 0, 0, size, CopyPixelOperation.SourceCopy);//对屏幕指定区域进行图像复制
            }
            return bitmap;
        }

    }
}
