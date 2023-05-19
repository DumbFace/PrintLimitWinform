using IWshRuntimeLibrary;
using Microsoft.Win32;
using PrintLimit.Services.ConfigurateServices;
using PrintLimit.Services.DALServices;
using PrintLimit.Services.EventServices;
using PrintLimit.Services.EventWatcherServices;
using PrintLimit.Services.RegisterEventServices;
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
        private readonly IConfigurateService configurateService;
        private readonly IEventLogPrintService eventLogPrintService;
        private readonly IEventWMIService eventWMIService;
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
            configurateService = new ConfigurateService();
            eventWMIService = new RegisterEvent();
            eventLogPrintService = new RegisterEvent();
            // Đặt form ở chế độ không hiển thị
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            // Bắt đầu công việc nền
            if (!configurateService.PreventMultipleThreadStart())
            {
                configurateService.CopyShortcutToStartup();
                configurateService.RegisterForceSetCopyCount();
                configurateService.EnableLogPrintService();

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

        public void DoWork()
        {
            string ip4Address = "";
            ip4Address = wMIService.GetIP();

            if (dALService.CheckEmployeeViaIP(ip4Address) != null)
            {
                eventWMIService.RegisterPrintJob();
                eventLogPrintService.RegisterLogPrintService();
            }
            else
            {
                MessageBox.Show("Bạn vui lòng tạo địa chỉ IP trên danh mục máy tính để thống kê in ấn");
                eventWatcherService.WatcherPrintJob();
            }
        }
    }

}
