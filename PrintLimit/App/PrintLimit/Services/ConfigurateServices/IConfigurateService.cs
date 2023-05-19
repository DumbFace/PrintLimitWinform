using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.ConfigurateServices
{
    interface IConfigurateService
    {
        bool RegisterForceSetCopyCount();
        bool CopyShortcutToStartup();
        bool PreventMultipleThreadStart();
        bool EnableLogPrintService();
    }
}
