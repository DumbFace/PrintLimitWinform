using IWshRuntimeLibrary;
using Microsoft.Win32;
using PrintLimit.Services.DALServices;
using PrintLimit.Services.EventWatcherServices;
using PrintLimit.Services.WMIServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrintLimit
{
    public class MyBackgroundApp : Form
    {
        private readonly IWMIService wMIService;
        private readonly IEventWatcherService eventWatcherService;
        private readonly IDALService dALService;

        //private readonly IWMIService wMIService;

        //private 
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new MyBackgroundApp());
        }

        public MyBackgroundApp()
        {
            //Dependencies
            wMIService = new WMIService();
            eventWatcherService = new EventWatcherService(wMIService);
            dALService = new DALService();

            // Đặt form ở chế độ không hiển thị
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            // Bắt đầu công việc nền
            if (!IsApplicationAlreadyRunning())
            {
                SetStartup();
                Thread workerThread = new Thread(DoWork);
                workerThread.Start();
            }
            else
            {
                MessageBox.Show("Ứng dụng đang chạy");
                Application.Exit();
                Environment.Exit(0);
            }
        }

        private void SetStartup()
        {
            var shell = new WshShell();

            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string sourceFile = @"C:\Program Files (x86)\VinaAi\PrintManager\PrintManager\PrintLimit.exe";
            string shortcutPath = Path.Combine(startupFolder, Path.GetFileNameWithoutExtension(sourceFile) + ".lnk");
            var shortcut = shell.CreateShortcut(shortcutPath) as IWshShortcut;

            try
            {

                shortcut.TargetPath = sourceFile;
                shortcut.Description = "PrintLimit.exe - Shortcut";
                shortcut.WorkingDirectory = startupFolder;

                shortcut.Save();
            }
            catch (IOException iox)
            {
                Console.WriteLine(iox.Message);
            }
        }

        static bool IsApplicationAlreadyRunning()
        {
            string processName = Process.GetCurrentProcess().ProcessName;
            Process[] processes = Process.GetProcessesByName(processName);
            return (processes.Length > 1);
        }

        public void DoWork()
        {
            string ip4Address = "";
            ip4Address = wMIService.GetIP();

            if (dALService.CheckEmployeeViaIP(ip4Address) != null)
            {
                eventWatcherService.WatcherPrintJob();
            }
            else
            {
                MessageBox.Show("Bạn vui lòng tạo địa chỉ IP trên danh mục máy tính để thống kê in ấn");
                eventWatcherService.WatcherPrintJob();
            }
        }
    }

}
