using HJJJJ.DeskReach.Plugins.FileTransfer.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.UI.Xaml.Controls;
using static HJJJJ.DeskReach.Windows.MainFrom;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace HJJJJ.DeskReach.Windows
{
    public partial class FileTransferForm : Form, IFileTransferViewContext
    {

        ListViewColumnSorter lvwColumnSorter = null;
        private Client client;
        private ConnectionType ConnectionMode;
        private FileTransferPlugin fileTransfer;
        public FileTransferForm(Client _client, ConnectionType type)
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            ConnectionMode = type;
            lvwColumnSorter = new ListViewColumnSorter();
            this.listView1.ListViewItemSorter = lvwColumnSorter;
            this.listView1.LargeImageList = imageList2;
            this.listView1.View = View.LargeIcon;
            this.client = _client;
            //根据控制模式按需初始化插件
            fileTransfer = new FileTransferPlugin(this);
            client.RegPlugin(fileTransfer);
        }

        private void FileTransferFrom_Load(object sender, EventArgs e)
        {
            ///更新本地目录结构
            foreach (string temp in Directory.GetLogicalDrives())
            {
                TreeNode rootNode = new TreeNode();
                rootNode.Text = temp;
                rootNode.Tag = new DirectoryInfo(temp);
                this.treeView1.Nodes.Add(rootNode);
                AddChildren(rootNode);
            }

            //更新远端目录结构
            fileTransfer.Action(new DirectoriesPacket(FunctionCode.GetLogicalDrivesReuqest));
        }

        public void UpdateDirectories(TreeNode node)
        {
            TreeNodeCollection nodes = treeView2.Nodes;

            FindNodesWithCondition(nodes, x => x.Text.StartsWith(node.Text), node);
            Console.WriteLine();
        }

        private void FindNodesWithCondition(TreeNodeCollection nodes, Func<TreeNode, bool> condition, TreeNode newNode)
        {
            if (treeView2.InvokeRequired)
            {
                treeView2.Invoke(new Action<TreeNodeCollection, Func<TreeNode, bool>, TreeNode>(FindNodesWithCondition), nodes, condition, newNode);
            }
            else
            {
                try
                {
                    foreach (TreeNode node in nodes)
                    {
                        if (condition(node))
                        {
                            node.Nodes.Clear();
                            node.Nodes.AddRange(newNode.Nodes.Cast<TreeNode>().ToArray());
                            node.Expand();
                        }

                        if (node.Nodes.Count > 0)
                        {
                            FindNodesWithCondition(node.Nodes, condition, newNode);
                        }
                    }
                }
                catch (Exception)
                {

                   
                }
     
            }

        }

        public void UpdatetLogicalDrives(List<TreeNode> nodes)
        {
            if (treeView2.InvokeRequired)
            {
                treeView2.Invoke(new Action<List<TreeNode>>(UpdatetLogicalDrives), nodes);
            }
            else
            {
                this.treeView2.Nodes.AddRange(nodes.ToArray());
            }

        }

        private void AddChildren(TreeNode parentNode)
        {
            try
            {
                DirectoryInfo dirInfo = (DirectoryInfo)parentNode.Tag;
                var d = dirInfo.GetDirectories();
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

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            foreach (TreeNode node in e.Node.Nodes)
            {
                if (node.Nodes.Count == 0)
                {
                    AddChildren(node);
                }
            }
        }

        /// <summary>
        /// toolStripDropDownButton中点击选择
        /// </summary>
        private void _Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuitem = (ToolStripMenuItem)sender;

            ToolStripDropDownItem panrentDropdown = (ToolStripDropDownItem)menuitem.OwnerItem;
            foreach (ToolStripMenuItem temp in panrentDropdown.DropDownItems)
            {
                temp.Checked = false;
            }

            menuitem.Checked = true;
            panrentDropdown.Text = menuitem.Text;
        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DirectoryInfo dirInfo = (DirectoryInfo)e.Node.Tag;
            listView1.Items.Clear();
            imageList2.Images.Clear();
            try
            {

                foreach (var file in dirInfo.GetFiles())
                {
                    AddListViewItem(file);
                }
            }
            catch (Exception ex)
            {

            }
        }

        int count = 0;
        void AddListViewItem(FileInfo file)
        {
            System.Windows.Forms.ListViewItem item = new System.Windows.Forms.ListViewItem();
            item.ImageIndex = 0;
            item.Text = file.Name;
            imageList2.Images.Add($"{count}", System.Drawing.Icon.ExtractAssociatedIcon(file.FullName).ToBitmap());
            item.ImageIndex = imageList2.Images.IndexOfKey($"{count}");
            count++;
            //item.SubItems.AddRange(new string[] { "asdsd", file.LastWriteTime.ToString() });
            this.listView1.Items.Add(item);
        }


        /// <summary>
        /// 排序
        /// </summary>
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {

            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // 重新设置此列的排序方法.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // 设置排序列，默认为正向排序
                lvwColumnSorter.SortColumn = e.Column;

                lvwColumnSorter.Order = SortOrder.Ascending;
            }
            // 用新的排序方法对ListView排序
            this.listView1.Sort();
        }


        private void toolStripSplitButton1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripSplitButton1_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "平铺":
                    this.listView1.View = View.Tile;
                    break;
                case "图标":
                    this.listView1.View = View.LargeIcon;
                    break;
                default:
                    break;
            }
        }

        private void treeView2_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //更新远端目录结构
            fileTransfer.Action(new DirectoriesPacket(FunctionCode.GetDirectoriesRequest, null, e.Node.FullPath.ToString()));
        }

        /// <summary>
        /// 发送文具
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendFileBtn_Click(object sender, EventArgs e)
        {
            var dirPath = treeView1.SelectedNode.FullPath;
            var files = listView1.SelectedItems.Cast<System.Windows.Forms.ListViewItem>().ToArray();
            var path = treeView2.SelectedNode.FullPath;
            foreach (var file in files)
            {
                var filePath = Path.Combine(dirPath, file.Text);
                fileTransfer.SendFile(filePath,path);
            }
        }
    }
}


class ListViewColumnSorter : IComparer
{
    private int ColumnToSort;// 指定按照哪个列排序      
    private SortOrder OrderOfSort;// 指定排序的方式               
    private CaseInsensitiveComparer ObjectCompare;// 声明CaseInsensitiveComparer类对象，
    public ListViewColumnSorter()// 构造函数
    {
        ColumnToSort = 0;// 默认按第一列排序            
        OrderOfSort = SortOrder.None;// 排序方式为不排序            
        ObjectCompare = new CaseInsensitiveComparer();// 初始化CaseInsensitiveComparer类对象
    }
    // 重写IComparer接口.        
    // <returns>比较的结果.如果相等返回0，如果x大于y返回1，如果x小于y返回-1</returns>
    public int Compare(object x, object y)
    {
        int compareResult;
        System.Windows.Forms.ListViewItem listviewX, listviewY;
        // 将比较对象转换为ListViewItem对象
        listviewX = (System.Windows.Forms.ListViewItem)x;
        listviewY = (System.Windows.Forms.ListViewItem)y;
        // 比较
        compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
        // 根据上面的比较结果返回正确的比较结果
        if (OrderOfSort == SortOrder.Ascending)
        {   // 因为是正序排序，所以直接返回结果
            return compareResult;
        }
        else if (OrderOfSort == SortOrder.Descending)
        {  // 如果是反序排序，所以要取负值再返回
            return (-compareResult);
        }
        else
        {
            // 如果相等返回0
            return 0;
        }
    }
    /// 获取或设置按照哪一列排序.        
    public int SortColumn
    {
        set
        {
            ColumnToSort = value;
        }
        get
        {
            return ColumnToSort;
        }
    }
    /// 获取或设置排序方式.    
    public SortOrder Order
    {
        set
        {
            OrderOfSort = value;
        }
        get
        {
            return OrderOfSort;
        }
    }
}


