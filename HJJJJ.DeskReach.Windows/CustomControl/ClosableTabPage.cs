using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HJJJJ.DeskReach.Windows
{
    public class ClosableTabPage : TabPage
    {
        private ContextMenuStrip contextMenuStrip;
        private Action CloseCallback;

        public ClosableTabPage(string text = "", string name = "", Action closeCallback = null)
        {
            Text = text;
            Name = name;
            CloseCallback = closeCallback;
            contextMenuStrip = new ContextMenuStrip();
            ToolStripMenuItem menuItem = new ToolStripMenuItem("Close " + Text);
            menuItem.Click += MenuItem_Click;
            contextMenuStrip.Items.Add(menuItem);
        }

        public void ShowContextMenu(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // 右键点击时显示右键菜单
                contextMenuStrip.Show(this, e.Location);
            }
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            // 处理右键菜单项点击事件
            CloseCallback?.Invoke();
        }


    }
}
