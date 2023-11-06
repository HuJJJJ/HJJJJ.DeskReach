using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Pointer
{
    public interface IPointerViewContext
    {
        /// <summary>
        /// 当前屏幕分辨率
        /// </summary>
      Rectangle Area { get; set; }
    }
}
