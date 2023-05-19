using PrintLimit.Services.JobServices;
using PrintLimit.Services.RegisterEventServices;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.EventServices
{
    class RegisterEvent : IEventLogPrintService, IEventWMIService
    {
        private readonly IJobService jobService;

        public RegisterEvent()
        {
            jobService = new JobService();
        }

        public void RegisterLogPrintService()
        {
            string logType = "Microsoft-Windows-PrintService/Operational";
            string query = "*[System[(EventID=805) or (EventID=307)]]";

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
