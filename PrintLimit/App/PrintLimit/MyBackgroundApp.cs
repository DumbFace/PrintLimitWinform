using IWshRuntimeLibrary;
using Microsoft.Extensions.Hosting;
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
    public class MyBackgroundApp : BackgroundService
    {
        private readonly IWMIService wMIService;
        private readonly IEventWatcherService eventWatcherService;
        private readonly IDALService dALService;
        private readonly IConfigurateService configurateService;
        private readonly IEventLogPrintService eventLogPrintService;
        private readonly IEventWMIService eventWMIService;
        private readonly IEventCreateSpooling eventCreateSpooling;
        private readonly IEventPrintSpool eventPrintSpool;
        //private readonly IWMIService wMIService;

        //private 
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        //static void Main()
        //{
        //    Application.Run(new MyBackgroundApp());
        //}

        public MyBackgroundApp()
        {
            //Dependencies
            wMIService = new WMIService();
            eventWatcherService = new EventWatcherService(wMIService);
            dALService = new DALService();
            configurateService = new ConfigurateService();
            eventWMIService = new RegisterEvent();
            eventLogPrintService = new RegisterEvent();
            eventPrintSpool = new RegisterEvent();
            eventCreateSpooling = new RegisterEvent();
            eventPrintSpool = new RegisterEvent();
        }

        public void DoWork()
        {
            string ip4Address = "";
            ip4Address = wMIService.GetIP();

            if (dALService.CheckEmployeeViaIP(ip4Address) != null)
            {
                //Copy spool file vào appdata
                eventPrintSpool.CreateSpoolPrint();

                //Giám sát in lấy thông tin tên file, loại giấy...
                eventWMIService.MonitorPrintJob();

                //Giám sám in trong window event nếu có sự kiện in thì lưu vào db
                eventLogPrintService.RegisterLogPrintService();
            }
            else
            {
                MessageBox.Show("Bạn vui lòng tạo địa chỉ IP trên danh mục máy tính để thống kê in ấn");
                eventPrintSpool.CreateSpoolPrint();
                eventWMIService.MonitorPrintJob();
                eventLogPrintService.RegisterLogPrintService();
            }
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DoWork();
            return Task.CompletedTask;
        }
    }

}
