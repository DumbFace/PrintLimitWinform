using Microsoft.Win32;
using PrintLimit.Services.DALServices;
using PrintLimit.Services.EventWatcherServices;
using PrintLimit.Services.WMIServices;
using System;
using System.Collections.Generic;
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
            Thread workerThread = new Thread(DoWork);
            workerThread.Start();


        }

        private void SetStartup()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            Assembly curAssembly = Assembly.GetExecutingAssembly();
            key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
        }

        public void DoWork()
        {
            string ip4Address = "";
            ip4Address = wMIService.GetIP();

            if (dALService.CheckEmployeeViaIP(ip4Address) != null)
            {
                eventWatcherService.WatcherPrintJob();
                SetStartup();
            }
            else
            {
                MessageBox.Show("Bạn vui lòng tạo địa chỉ IP trên danh mục máy tính");
                eventWatcherService.WatcherPrintJob();
                SetStartup();
            }
        }
    }

}
