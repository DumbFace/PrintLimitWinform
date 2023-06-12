using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RegiserServicePrintLimit
{
    internal class Program
    {
        //private static readonly string filePath = "D:\\Repo\\Github\\PrintLimitWinform\\PrintLimit\\App\\PrintLimit\\bin\\Release\\PrintLimit.exe";
        private static readonly string filePath = "C:\\Program Files (x86)\\VinaAi\\PrintManager\\PrintLimit.exe";

        static void Main(string[] args)
        {
            //MessageBox.Show("Services has registed!!!!!");
            CreateService();
            StartService();
        }

        public static void CreateService()
        {

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"create PrintLimit binPath=\"{filePath}\" start= auto",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            //Log.Information("Tạo Service", process.StartInfo);
            bool result = process.Start();
        }

        public static void StartService()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = "start PrintLimit",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            //Log.Information("Chạy Service", process.StartInfo);
            bool result = process.Start();
        }
    }
}
