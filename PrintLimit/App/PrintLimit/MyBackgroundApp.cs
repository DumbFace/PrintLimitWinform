﻿using IWshRuntimeLibrary;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using PrintLimit.Services.ConfigurateServices;
using PrintLimit.Services.DALServices;
using PrintLimit.Services.EventServices;
using PrintLimit.Services.EventWatcherServices;
using PrintLimit.Services.RegisterEventServices;
using PrintLimit.Services.SerilogServices;
using PrintLimit.Services.WMIServices;
using Serilog;
using System;
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
        private readonly IWriteSerilog _logger;

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
            _logger = new SerilogService();

            //Khởi tạo serilog
            if (!configurateService.PreventMultipleThreadStart())
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("microsoft", Serilog.Events.LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.File("C:/log/", rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            }
        }

        public void DoWork()
        {
            string ip4Address = "";
            ip4Address = wMIService.GetIP();
            //Configurate Log Event Viewer
            configurateService.EnableLogPrintService();

            //Copy spool file vào appdata
            eventPrintSpool.CreateSpoolPrint();

            //Giám sát in lấy thông tin tên file, loại giấy...
            eventWMIService.MonitorPrintJob();

            //Giám sám in trong window event nếu có sự kiện in thì lưu vào db
            eventLogPrintService.RegisterLogPrintService();

            if (dALService.CheckEmployeeViaIP(ip4Address) == null)
            {
                MessageBox.Show("Bạn vui lòng tạo địa chỉ IP trên danh mục máy tính để thống kê in ấn");

            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!configurateService.PreventMultipleThreadStart())
            {
                try
                {
                    DoWork();
                }
                catch (Exception ex)
                {
                    _logger.WriteLogHeader("Error");
                    Log.Error(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Giám sát máy in đang hoạt động");
                Application.Exit();
                Environment.Exit(0);
            }

        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {

            _logger.WriteLogInfo("Service", "Started!");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.WriteLogInfo("Service", "Stopped!");
            return base.StopAsync(cancellationToken);
        }
    }

}
