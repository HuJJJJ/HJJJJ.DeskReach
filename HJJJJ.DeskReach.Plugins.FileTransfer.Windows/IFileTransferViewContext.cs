using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HJJJJ.DeskReach.Plugins.FileTransfer.Windows
{

    public interface IFileTransferViewContext
    {

        void UpdateDirectories(TreeNode node);
        void UpdatetLogicalDrives(List<TreeNode> nodes);
    }
}
