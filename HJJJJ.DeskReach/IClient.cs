using System;

namespace HJJJJ.DeskReach
{
    public abstract class IClient
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="request"></param>
        public void Send(byte[] data) => SendBytes(data);

        protected abstract void SendBytes(byte[] bytes);
        protected abstract void ReceivedBytes(BasePackage response);

    }
}
