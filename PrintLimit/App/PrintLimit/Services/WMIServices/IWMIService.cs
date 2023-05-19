using PrintLimit.Data.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.WMIServices
{
    interface IWMIService
    {
        string GetIP();
        void GetPrintJob(object sender, EventArrivedEventArgs e);


    }
}
