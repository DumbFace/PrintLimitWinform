using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.JobServices
{
    interface IJobService
    {
        void PrintJob(object sender, EventArrivedEventArgs e);
        void LogPrintServiceJob(object sender, EventRecordWrittenEventArgs e);
        void MonitorSpoolingJob(object sender, EventArrivedEventArgs e);
        void OnSpoolingCreated(object source, FileSystemEventArgs e);
    }
}
