using HJJJJ.DeskReach.Plugins.Pointer;
using System;
using System.Collections.Generic;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Keyboard
{
    public class KeyboardPlugin:BasePlugin
    {
        private IKeyboardViewContext ViewContext { get; set; }

        public KeyboardPlugin(IKeyboardViewContext viewContext)
        {
            ViewContext = viewContext;
        }


        /// <summary>
        /// 操作键盘
        /// </summary>
        public void Action(KeyboardPacket packet) => Send(packet);

        public void SendKey(string keyChar) 
        {
        ViewContext.SendKey(keyChar);
        }
    }
}
