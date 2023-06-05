using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.RegisterEventServices
{
    interface IEventWMIService
    {
        void RegisterPrintJob();
        void MonitorPrintJob();
    }
}
