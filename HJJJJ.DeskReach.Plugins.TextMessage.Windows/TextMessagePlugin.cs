using HJJJJ.DeskReach.Plugins.Pointer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HJJJJ.DeskReach.Plugins.TextMessage.Windows
{
    public class TextMessagePlugin : BasePlugin
    {
        private ITextMessageViewContext ViewContext { get; set; }


        public new event EventHandler<byte[]> OnDataReceived;
        public TextMessagePlugin(ITextMessageViewContext viewContext)
        {
            ViewContext = viewContext;
            this.OnDataReceived += TextMessagePlugin_OnDataReceived;
        }


        /// <summary>
        /// 接收消息处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void RaiseDataReceived(object sender, byte[] e) => SafelyInvokeCallback(() => { OnDataReceived?.Invoke(sender, e); });
        private void TextMessagePlugin_OnDataReceived(object sender, byte[] e)
        {
            var data = new TextMessagePacket(e);
            switch ((TextMessageActionType)data.Code)
            {
                case TextMessageActionType.SendMessage:
                    ViewContext.ShowMessage(data.Message);
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// 操作鼠标
        /// </summary>
        public void Action(TextMessagePacket packet) => Send(packet);
    }
}
