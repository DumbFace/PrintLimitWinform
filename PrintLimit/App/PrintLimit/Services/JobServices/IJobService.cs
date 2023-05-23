using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
        void CreateSpoolingJob(object sender, EventArrivedEventArgs e);
    }
}
