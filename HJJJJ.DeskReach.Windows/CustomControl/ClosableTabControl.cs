using HJJJJ.DeskReach.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HJJJJ.DeskReach.Windows
{
    public partial class ClosableTabControl : TabControl
    {
        public ClosableTabControl()
        {
            if (!DesignMode)
            {
                // 注册 MouseDown 事件处理程序
                MouseDown += ClosableTabControl_MouseDown;
            }
        }

        private void ClosableTabControl_MouseDown(object sender, MouseEventArgs e)
        {
            // 确保控件处于运行时才处理鼠标事件
            if (DesignMode)
                return;
            // 获取当前点击的选项卡索引
            for (int i = 0; i < TabCount; i++)
            {
                Rectangle rect = GetTabRect(i);
                if (rect.Contains(e.Location) && e.Button == MouseButtons.Right)
                {
                    // 获取选项卡的自定义页面对象
                    ClosableTabPage tabPage = TabPages[i] as ClosableTabPage;
                    if (tabPage != null)
                    {
                        // 在选项卡上的鼠标按下事件中显示右键菜单
                        tabPage.ShowContextMenu(sender, e);
                    }
                    break;
                }
            }
        }
    }
}
