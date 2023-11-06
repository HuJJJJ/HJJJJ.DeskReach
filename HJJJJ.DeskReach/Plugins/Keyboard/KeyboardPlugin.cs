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

        public void SendKey(string keyChar) 
        {
        ViewContext.SendKey(keyChar);
        }
    }
}
