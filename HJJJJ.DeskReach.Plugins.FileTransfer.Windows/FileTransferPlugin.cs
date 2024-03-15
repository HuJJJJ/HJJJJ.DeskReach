using ApeFree.Protocol.ApeFtp;
using Shell32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HJJJJ.DeskReach.Plugins.FileTransfer.Windows
{
    public class FileTransferPlugin : BasePlugin
    {

        private IFileTransferViewContext ViewContext;

        public new event EventHandler<byte[]> OnDataReceived;

        private ApeFtpSender ftpSender;
        private ApeFtpReceiver ftpReceiver;
        public FileTransferPlugin(IFileTransferViewContext viewContext)
        {
            ViewContext = viewContext;
            OnDataReceived += FileTransferPlugin_OnDataReceived;
            ftpSender = new ApeFtpSender((bytes) =>
            {
                Action(new FileTransferPacket(FunctionCode.Transfer, bytes));
                //  client.Send(bytes);
            });
            ftpReceiver = new SimpleReceiver((bytes) =>
            {

                Action(new FileTransferPacket(FunctionCode.Transfer, bytes));
            });
        }

        /// <summary>
        /// 接收消息处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void RaiseDataReceived(object sender, byte[] e) => SafelyInvokeCallback(() => { OnDataReceived?.Invoke(sender, e); });

        private void FileTransferPlugin_OnDataReceived(object sender, byte[] e)
        {
            switch ((FunctionCode)e[0])
            {
                case FunctionCode.Transfer:
                    Transfer(new FileTransferPacket(e));
                    break;
                case FunctionCode.RenameFiles:
                    var fileTransferPacket = new FileTransferPacket(e);
                    var sessionDir = Path.Combine(ftpReceiver.TransferCachePath, fileTransferPacket.SessionId);
                    var filePath = Path.Combine(sessionDir, fileTransferPacket.SessionId);
                    File.Move(filePath, Path.Combine(ftpReceiver.TransferCachePath, fileTransferPacket.FileName));
                    Directory.Delete(sessionDir, true);
                    break;
                case FunctionCode.GetLogicalDrivesReuqest:
                    Action(new DirectoriesPacket(FunctionCode.GetLogicalDrivesResponse, GetLogicalDrives()));
                    break;
                case FunctionCode.GetLogicalDrivesResponse:
                    var getLogicalDrivesResponse = new DirectoriesPacket(e);
                    ViewContext.UpdatetLogicalDrives(getLogicalDrivesResponse.TreeNodes);
                    break;
                case FunctionCode.GetDirectoriesRequest:
                    var dirPath = new DirectoriesPacket(e);
                    var treeNode = AddChildren(new DirectoryInfo(dirPath.Path));
                    Action(new DirectoriesPacket(FunctionCode.GetDirectoriesResponse, new List<TreeNode>() { treeNode }));
                    break;
                case FunctionCode.GetDirectoriesResponse:
                    var getDirectoriesResponse = new DirectoriesPacket(e);
                    ViewContext.UpdateDirectories(getDirectoriesResponse.TreeNodes[0]);
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// 获取本地逻辑驱动器
        /// </summary>
        /// <returns></returns>
        public List<TreeNode> GetLogicalDrives()
        {
            List<TreeNode> treeNodes = new List<TreeNode>();
            foreach (string temp in Directory.GetLogicalDrives())
            {
                TreeNode rootNode = new TreeNode();
                rootNode.Text = temp;
                rootNode.Tag = new DirectoryInfo(temp);
                treeNodes.Add(rootNode);
                AddChildren(rootNode);
            }
            return treeNodes;
        }

        /// <summary>
        /// 获取子节点目录
        /// </summary>
        /// <param name="parentNode"></param>
        private void AddChildren(TreeNode parentNode)
        {
            try
            {
                DirectoryInfo dirInfo = (DirectoryInfo)parentNode.Tag;
                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    TreeNode node = new TreeNode();
                    //FileInfo fif = new FileInfo(dir.FullName);
                    //node.Text = fif.Name; 
                    //if (File.Exists(dir.FullName))
                    node.Text = dir.Name;
                    //else
                    //{
                    //    Directory.GetParent(dir.FullName);
                    //}
                    node.Tag = dir;
                    parentNode.Nodes.Add(node);
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 获取子节点目录
        /// </summary>
        /// <param name="parentNode"></param>
        private TreeNode AddChildren(DirectoryInfo dirInfo)
        {
            try
            {
                TreeNode parentNode = new TreeNode();
                parentNode.Text = dirInfo.Name;
                parentNode.Tag = dirInfo;
                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    TreeNode node = new TreeNode();
                    //FileInfo fif = new FileInfo(dir.FullName);
                    //node.Text = fif.Name; 
                    //if (File.Exists(dir.FullName))
                    node.Text = dir.Name;
                    //else
                    //{
                    //    Directory.GetParent(dir.FullName);
                    //}
                    node.Tag = dir;
                    parentNode.Nodes.Add(node);
                }
                return parentNode;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public void Transfer(FileTransferPacket packet)
        {
            CommandCode command = (CommandCode)packet.Data.FirstOrDefault();
            switch (command)
            {
                case CommandCode.DemandRequest:

                    if (ftpReceiver.TransferCachePath == null) 
                    {
                        var savePath = GetSavePath();
                        ftpReceiver.TransferCachePath = savePath;
                    }
                    ftpReceiver.Input(packet.Data);
                    break;
                case CommandCode.TransferRequest:
                    ftpReceiver.Input(packet.Data);
                    break;
                case CommandCode.TransferResponse:
                    var resp = new TransferResponse(packet.Data);
                    if ((ResultCode)packet.Data.ElementAt(21) == ResultCode.Completed)
                    {
                        var session = ftpSender.GetSession(resp.Md5, resp.TotalLength);
                        Action(new FileTransferPacket(FunctionCode.RenameFiles, new byte[0], Path.GetFileName(session.FilePath), $"{session.MD5.ToHexString()}-{session.TotalLength}"));
                        ftpReceiver.TransferCachePath = null;
                    }
                    ftpSender.Input(packet.Data);
                    break;
                default:
                    break;
            }
        }


        [DllImport("user32.dll")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public static bool IsDesktopWindow(IntPtr windowHandle)
        {
            StringBuilder className = new StringBuilder(256);
            GetClassName(windowHandle, className, className.Capacity);
            return className.ToString() == "Progman";
        }

        public static bool IsDesktopWindowAtForeground()
        {
            IntPtr windowHandle = GetForegroundWindow();
            return IsDesktopWindow(windowHandle);
        }


        private string GetSavePath()
        {
            //判断鼠标指针是否在桌面
            if (IsDesktopWindowAtForeground())
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string filename;
            SHDocVw.InternetExplorer activeWindow = null;

            foreach (SHDocVw.InternetExplorer window in new SHDocVw.ShellWindows())
            {
                filename = Path.GetFileNameWithoutExtension(window.FullName).ToLower();
                if (filename.ToLowerInvariant() == "explorer" || filename.ToLowerInvariant() == "资源管理器")
                {
                    var folder = (Shell32.Folder2)((Shell32.IShellFolderViewDual2)window.Document).Folder;
                    string folderPath = folder.Self.Path;

                    // 获取当前活动窗口的句柄
                    IntPtr foregroundWindowHandle = GetForegroundWindow();

                    // 将窗口句柄转换为 int 类型进行比较
                    int foregroundWindowHWND = foregroundWindowHandle.ToInt32();

                    // 判断当前资源管理器窗口是否为活动窗口
                    if (window.HWND == foregroundWindowHWND)
                    {
                        Shell32.FolderItems items = ((Shell32.IShellFolderViewDual2)window.Document).SelectedItems();
                        if (items.Count > 0)
                        {
                            var itemPath = items.Item(0).Path;
                            if (Directory.Exists(itemPath))
                            {
                                folderPath = itemPath;
                            }
                        }
                        activeWindow = window;
                        return folderPath;
                    }
                }
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        /// <summary>
        /// 文本操作
        /// </summary>
        public void Action(BasePacket packet) => Send(packet);

        public override void RegInit(Client client)
        {
            base.RegInit(client);
        }

        public void SendFile(string filePath,string savePath="")
        {
            if (savePath != "") 
            {
                Action(new FileTransferPacket(FunctionCode.SetSaveDirectory, new byte[0], savePath));
            }
            ftpSender.SendFile(filePath);
        }
    }
}
