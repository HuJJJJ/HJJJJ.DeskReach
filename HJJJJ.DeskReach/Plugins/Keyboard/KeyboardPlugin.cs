using HJJJJ.DeskReach.Plugins.Pointer;
using System;
using System.Collections.Generic;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.Keyboard
{
    public class KeyboardPlugin : BasePlugin
    {
        private IKeyboardViewContext ViewContext { get; set; }
        private new event EventHandler<byte[]> OnDataReceived;

        public KeyboardPlugin(IKeyboardViewContext viewContext)
        {
            ViewContext = viewContext;
            OnDataReceived += KeyboardPlugin_OnDataReceived;
        }

        /// <summary>
        /// 接收消息处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void RaiseDataReceived(object sender, byte[] e) => SafelyInvokeCallback(() => { OnDataReceived?.Invoke(sender, e); });

        /// <summary>
        /// 插件接收消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void KeyboardPlugin_OnDataReceived(object sender, byte[] e)
        {
            var data = new KeyboardPacket(e);
            switch (data.Code)
            {
                case KeyboardActionType.Single:
                    ViewContext.SendKey(data.KeyChars);
                    break;
                case KeyboardActionType.Combination:
                    break;
            }
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
