using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.API_Win32
{
    public static class WinpoolDLL
    {
        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetJob(IntPtr hPrinter, uint JobId, uint Level, IntPtr pJob, uint cbBuf, out uint pcbNeeded);

        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool ClosePrinter(IntPtr hPrinter);
    }
}
