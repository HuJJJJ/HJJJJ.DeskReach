using HJJJJ.DeskReach.Plugins.Pointer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

namespace HJJJJ.DeskReach.Demo
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {


            //var list = new List<stu>()
            //{
            //    new stu("asdsd",2),
            //    new stu("asdsd",22),
            //    new stu("asdsd",435),
            //    new stu("asdsd",2),
            //    new stu("asdsd",2),
            //};
            //byte[] bytess = null;
            //BinaryFormatter bf = new BinaryFormatter();
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    bf.Serialize(ms, list);
            //    bytess = ms.ToArray();
            //}
            //using (MemoryStream ms = new MemoryStream(bytess))
            //{
            //    List<stu> a = (List<stu>)bf.Deserialize(ms);
            //    Console.WriteLine();
            //}

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainFrom());
        }
    }
    [Serializable]
    public class stu
    {

        public stu(string id, int age)
        {
            this.id = id;
            this.age = age;
        }
        public string id { get; set; }
        public int age { get; set; }
    }
}
