using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HJJJJ.DeskReach.Plugins.FileTransfer.Windows
{
    public enum FileTransferActionType
    {
        // =============== 请求类命令区间 ===============
        DemandRequest = 0x01,
        TransferRequest = 0x02,
        // =============== 响应类命令区间 ===============
        TransferResponse = 0x03
    }
}
