using PrintLimit.Services.JobServices;
using PrintLimit.Services.RegisterEventServices;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.EventServices
{
    class RegisterEvent : IEventLogPrintService, IEventWMIService, IEventCreateSpooling, IEventPrintSpool
    {
        private readonly IJobService jobService;

        public RegisterEvent()
        {
            jobService = new JobService();
        }

        public void MonitorPrintJob()
        {
            // Tạo WMI query để theo dõi sự kiện thêm công việc in
            string queryString =
                "SELECT * " +
                "FROM __InstanceCreationEvent WITHIN 0.1 " +
                "WHERE TargetInstance ISA 'Win32_PrintJob'";

            // Sử dụng WMI namespace 'CIMV2'
            string scope = @"\\localhost\root\CIMV2";

            // Tạo event watcher và gán các sự kiện
            ManagementEventWatcher watcher = new ManagementEventWatcher(scope, queryString);

            // Sự kiện xảy ra khi một công việc in mới được thêm vào hàng đợi
            watcher.EventArrived += new EventArrivedEventHandler(this.jobService.MonitorSpoolingJob);

            // Bắt đầu giám sát sự kiện
            watcher.Start();
        }

        public void CreateSpoolPrint()
        {
            var watcher = new FileSystemWatcher();

            watcher.Path = @"C:\Windows\System32\spool\PRINTERS";

            // Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories. 
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            // Only watch text files.
            watcher.Filter = "*.SPL";

            // Add event handlers.
            watcher.Changed += jobService.OnSpoolingCreated;

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        public void RegisterLogPrintService()
        {
            string logType = "Microsoft-Windows-PrintService/Operational";
            string query = "*[System[(EventID=307)]]";

            EventLogQuery eventsQuery = new EventLogQuery(logType, PathType.LogName, query);

            EventLogWatcher logWatcher = new EventLogWatcher(eventsQuery);

            logWatcher.EventRecordWritten += new EventHandler<EventRecordWrittenEventArgs>(this.jobService.LogPrintServiceJob);

            logWatcher.Enabled = true;
        }

        public void RegisterPrintJob()
        {
            try
            {
                string ComputerName = "localhost";
                string WmiQuery;
                ManagementEventWatcher Watcher;
                ManagementScope Scope;


                if (!ComputerName.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                {
                    ConnectionOptions Conn = new ConnectionOptions();
                    Conn.Username = "";
                    Conn.Password = "";
                    Conn.Authority = "ntlmdomain:DOMAIN";
                    Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", ComputerName), Conn);
                }
                else
                    Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", ComputerName), null);
                Scope.Connect();

                string wqlQuery = @"Select * From __InstanceModificationEvent  Within 0.1
                Where TargetInstance ISA 'Win32_PrintJob' And TargetInstance.JobStatus = 'Printing'";

                Watcher = new ManagementEventWatcher(Scope, new EventQuery(wqlQuery));
                Watcher.EventArrived += new EventArrivedEventHandler(this.jobService.PrintJob);
                Watcher.Start();
            }
            catch (Exception e)
            {
            }
        }
    }
}
