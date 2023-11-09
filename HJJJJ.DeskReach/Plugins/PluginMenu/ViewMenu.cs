using System;
using System.Collections.Generic;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.PluginMenu
{
    public class ViewMenu
    {
        public List<MenuItem> MenuItems { get; set; }
        public string PluginName { get; set; }

        public ViewMenu()
        {
            MenuItems =new List<MenuItem>();
        }
    }

    public class MenuItem
    {     
        /// <summary>
           /// 回调
           /// </summary>
        public delegate void CallbackDelegate(object e);
        private CallbackDelegate Callback;
        public MenuItem(CallbackDelegate callback)
        {
            Callback = callback;
        }

        public void SafelyInvokeCallback(object obj)
        {
            try
            {
                Callback.Invoke(obj);
            }
            catch (Exception e)
            {
                Console.WriteLine("错误--------------------------------：" + e.Message + "-------------------------------------------");
                throw;
            }
        }

        /// <summary>
        /// 空间的文本
        /// </summary>
        public string Text { get; set; }
        public MenuItemType Type { get; set; }

        
    }

    public enum MenuItemType
    {
        Input,
        Button,
        Lable
    }
}
