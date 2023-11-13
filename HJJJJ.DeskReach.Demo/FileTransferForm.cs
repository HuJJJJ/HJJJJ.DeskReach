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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace HJJJJ.DeskReach.Demo
{
    public partial class FileTransferForm : Form
    {

        ListViewColumnSorter lvwColumnSorter = null;

        public FileTransferForm()
        {
            InitializeComponent();
            lvwColumnSorter = new ListViewColumnSorter();
            this.listView1.ListViewItemSorter = lvwColumnSorter;
            this.listView1.LargeImageList = imageList2;
        }

        private void FileTransferFrom_Load(object sender, EventArgs e)
        {
            foreach (string temp in Directory.GetLogicalDrives())
            {
                TreeNode rootNode = new TreeNode();
                rootNode.Text = temp;
                rootNode.Tag = new DirectoryInfo(temp);
                this.treeView1.Nodes.Add(rootNode);
                AddChildren(rootNode);
            }
        }


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

        /// <summary>
        /// 将目录内容添加到ListView中
        /// </summary>
        /// 
        // FileInfo Fif = new FileInfo(childDir.FullName);
        //  string fileSize = (Fif.Length / 1024).ToString() + "kb";
        /*
        string fileSize;
        long length = GetDirectoryLength(childDir.FullName) / 1024;
        if (length > 1024)
            fileSize = (length / 1024).ToString() + "M";
        else
            fileSize = length.ToString() + "kb";
        */
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
                //var dirs = dirInfo.GetDirectories();
                //if (dirs.Count() == 0)
                //{
                //    foreach (var file in dirInfo.GetFiles())
                //    {
                //        AddListViewItem(file);
                //    }
                //}
                //else
                //{
                //    foreach (DirectoryInfo childDir in dirs)
                //    {
                //        foreach (var file in childDir.GetFiles())
                //        {
                //            AddListViewItem(file);
                //        }

                //    }
                //}

            }
            catch (Exception ex)
            {

            }
        }

        int count = 0;
        void AddListViewItem(FileInfo file)
        {
            ListViewItem item = new ListViewItem();
            item.ImageIndex = 0;
            item.Text = file.Name;
            imageList2.Images.Add($"{count}", System.Drawing.Icon.ExtractAssociatedIcon(file.FullName).ToBitmap());
            item.ImageIndex = imageList2.Images.IndexOfKey($"{count}");
            count++;
            //item.SubItems.AddRange(new string[] { "asdsd", file.LastWriteTime.ToString() });
            this.listView1.Items.Add(item);
        }

        public void SelectImageList()
        {

        }


        public void fileType()
        {

        }

        //二进制下的文件头。主要是前两位




        /*
        /// <summary>
        /// 获取文件的大小。
        /// </summary>
        public long GetDirectoryLength(string dirPath)
        {
            long len = 0;
            //判断该路径是否存在（是否为文件夹） 
            if (!Directory.Exists(dirPath))
            {
                //查询文件的大小 
                len = FileSize(dirPath);
            }
            else
            {
                //定义一个DirectoryInfo对象 
                DirectoryInfo di = new DirectoryInfo(dirPath);

                //通过GetFiles方法，获取di目录中的所有文件的大小 
                foreach (FileInfo fi in di.GetFiles())
                {
                    len += fi.Length;
                }
                //获取di中所有的文件夹，并存到一个新的对象数组中，以进行递归 
                DirectoryInfo[] dis = di.GetDirectories();
                if (dis.Length > 0)
                {
                    for (int i = 0; i < dis.Length; i++)
                    {
                        len += GetDirectoryLength(dis[i].FullName);
                    }
                }
            }
            return len;
        }
        public static long FileSize(string filePath)
        {
            //定义一个FileInfo对象，是指与filePath所指向的文件相关联，以获取其大小 
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        } 
        */

        /// <summary>
        /// 改变ListView中的显示方式。
        /// </summary>
        private void CheckedChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 排序
        /// </summary>
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //ColumnHeader ch = listView1.Columns[e.Column];
            //switch (ch.Text)
            //{
            //    case "名称":
            //        {
            //            if (listView1.Sorting != SortOrder.Descending)
            //                listView1.Sorting = SortOrder.Descending;
            //            else
            //                listView1.Sorting = SortOrder.Ascending;
            //            break;
            //        }
            //    case "修改时间":
            //        {
            //            listView1.Sort();
            //            //if (listView1.Sorting != SortOrder.Descending)
            //            //    listView1.Sorting = SortOrder.Descending;
            //            //else
            //            //    listView1.Sorting = SortOrder.Ascending;
            //            break;
            //        }
            //    default:
            //        break;
            //}

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
                case "详细信息":
                    this.listView1.View = View.Details;
                    break;
                default:
                    break;
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
        ListViewItem listviewX, listviewY;
        // 将比较对象转换为ListViewItem对象
        listviewX = (ListViewItem)x;
        listviewY = (ListViewItem)y;
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


