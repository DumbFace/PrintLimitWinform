using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Structure
{
    [StructLayout(LayoutKind.Sequential)]
    public class JOB_INFO_2
    {
        public uint JobId;
        public string pPrinterName;
        public string pMachineName;
        public string pUserName;
        public string pDocument;
        public string pNotifyName;
        public string pDatatype;
        public string pPrintProcessor;
        public string pParameters;
        public string pDriverName;
        public IntPtr pDevMode;
        public string pStatus;
        public IntPtr pSecurityDescriptor;
        public uint Status;
        public uint Priority;
        public uint Position;
        public uint StartPage;
        public uint UntilPage;
        public uint TotalPages;
        public uint Size;
        public SYSTEMTIME Submitted;
        public uint Time;
        public uint PagesPrinted;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        public ushort Year;
        public ushort Month;
        public ushort DayOfWeek;
        public ushort Day;
        public ushort Hour;
        public ushort Minute;
        public ushort Second;
        public ushort Milliseconds;
    }
}
