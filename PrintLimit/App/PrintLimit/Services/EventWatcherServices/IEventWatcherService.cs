using PrintLimit.Data.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.EventWatcherServices
{
    interface IEventWatcherService
    {
        void WatcherPrintJob();
    }
}
