using System.Diagnostics;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Command command = new Command();
            command.Output += Command_Output;
            command.Error += Command_Error;
            command.Exited += Command_Exited;
            while (true)
            {
                string cmd = Console.ReadLine();
                command.RunCMD(cmd);
            }

        }

        private static void Command_Exited()
        {
            Console.WriteLine("已退出");
        }

        private static void Command_Error(string msg)
        {
            Console.Write(msg);
        }

        private static void Command_Output(string msg)
        {
            Console.Write(msg);
        }

        }
    }
