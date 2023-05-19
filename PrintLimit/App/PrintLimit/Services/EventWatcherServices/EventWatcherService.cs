using PrintLimit.Data.EF;
using PrintLimit.Services.WMIServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.EventWatcherServices
{
    class EventWatcherService : IEventWatcherService
    {
        private readonly IWMIService wMIService;
        public EventWatcherService(IWMIService _wMIService)
        {
            wMIService = _wMIService;
        }

        public void WatcherPrintJob()
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

                WmiQuery = "Select * From __InstanceOperationEvent Within 0.01 " +
                "Where TargetInstance ISA 'Win32_PrintJob' ";
                string wqlQuery = @"Select * From __InstanceModificationEvent  Within 0.1
                Where TargetInstance ISA 'Win32_PrintJob' And TargetInstance.JobStatus = 'Printing'";
                Watcher = new ManagementEventWatcher(Scope, new EventQuery(wqlQuery));
                Watcher.EventArrived += new EventArrivedEventHandler(this.wMIService.GetPrintJob);
                Watcher.Start();
            }
            catch (Exception e)
            {
            }
        }
    }
}
