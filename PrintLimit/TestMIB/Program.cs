using System;
using System.Runtime.InteropServices;

public class PrintJob
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct DEVMODE
    {

        private const int CCHDEVICENAME = 32;
        private const int CCHFORMNAME = 32;
        private const int CCHBINNAME = 24;


        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
        public string dmDeviceName;

        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public DM dmFields;

        public short dmOrientation;
        public short dmPaperSize;
        public short dmPaperLength;
        public short dmPaperWidth;
        public short dmScale;
        public short dmCopies;
        public short dmDefaultSource;
        public short dmPrintQuality;

        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmNup;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    [Flags]
    public enum DM : int
    {
        Orientation = 0x1,
        PaperSize = 0x2,
        PaperLength = 0x4,
        PaperWidth = 0x8,
        Scale = 0x10,
        Position = 0x20,
        NUP = 0x40,
        DisplayOrientation = 0x80,
        Copies = 0x100,
        DefaultSource = 0x200,
        PrintQuality = 0x400,
        Color = 0x800,
        Duplex = 0x1000,
        YResolution = 0x2000,
        TTOption = 0x4000,
        Collate = 0x8000,
        FormName = 0x10000,
        LogPixels = 0x20000,
        BitsPerPixel = 0x40000,
        PelsWidth = 0x80000,
        PelsHeight = 0x100000,
        DisplayFlags = 0x200000,
        DisplayFrequency = 0x400000,
        ICMMethod = 0x800000,
        ICMIntent = 0x1000000,
        MediaType = 0x2000000,
        DitherType = 0x4000000,
        PanningWidth = 0x8000000,
        PanningHeight = 0x10000000,
        DisplayFixedOutput = 0x20000000,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JOB_INFO_2
    {
        public uint JobId;
        public IntPtr pPrinterName;
        public IntPtr pMachineName;
        public IntPtr pUserName;
        public IntPtr pDocument;
        public IntPtr pNotifyName;
        public IntPtr pDataType;
        public IntPtr pPrintProcessor;
        public IntPtr pParameters;
        public IntPtr pDriverName;
        public IntPtr pDevmode;
        public IntPtr pStatus;
        public IntPtr pSecurityDescriptor;
        public uint Status;
        public uint Priority;
        public uint Position;
        public uint StartTime;
        public uint UntilTime;
        public uint TotalPages;
        public uint Size;
        public uint Time;
        public uint PagesPrinted;
    }

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool GetJob(IntPtr hPrinter, uint JobId, uint Level, IntPtr pJob, uint cbBuf, out uint pcbNeeded);

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool ClosePrinter(IntPtr hPrinter);

    public static void Main()
    {
        IntPtr hPrinter;
        string printerName = "HP LaserJet MFP M227-M231 PCL-6";
        uint jobId = 108;  // replace with actual JobId

        if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
            throw new Exception("Failed to open printer.");

        uint bytesNeeded;
        GetJob(hPrinter, jobId, 2, IntPtr.Zero, 0, out bytesNeeded);

        if (Marshal.GetLastWin32Error() != 122)  // ERROR_INSUFFICIENT_BUFFER
            throw new Exception("Failed to get job info.");

        IntPtr pJobInfo = Marshal.AllocHGlobal((int)bytesNeeded);

        if (GetJob(hPrinter, jobId, 2, pJobInfo, bytesNeeded, out bytesNeeded))
        {
            JOB_INFO_2 jobInfo = (JOB_INFO_2)Marshal.PtrToStructure(pJobInfo, typeof(JOB_INFO_2));
            DEVMODE devMode = (DEVMODE)Marshal.PtrToStructure(jobInfo.pDevmode, typeof(DEVMODE));

            // Do what you need with devMode...

            Marshal.FreeHGlobal(pJobInfo);
        }
        else
        {
            throw new Exception("Failed to get job info.");
        }

        ClosePrinter(hPrinter);
    }
}