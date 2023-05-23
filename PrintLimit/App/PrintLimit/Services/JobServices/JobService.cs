using PrintLimit.Data.EF;
using PrintLimit.Services.CachingServices;
using PrintLimit.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Management;
using System.Printing;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using PrintLimit.API_Win32;
using PrintLimit.Models;

namespace PrintLimit.Services.JobServices
{
    class JobService : IJobService
    {
        const string ID_NHAN_VIEN = "idNhanVien";
        private readonly ICachingService cachingService;
        public JobService()
        {
            cachingService = new CachingService();
        }

        public void CreateSpoolingJob(object sender, EventArrivedEventArgs e)
        {
            var printJobDocument = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Document"];
            var printJobPaperSize = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["PaperSize"];
            var printJobName = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"];
            var printJobId = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["JobId"];
            var printJobTotalPages = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["TotalPages"];

            string document = printJobDocument != null ? printJobDocument.ToString() : "";
            string paperSize = printJobPaperSize != null ? printJobPaperSize.ToString() : "";
            string name = printJobName != null ? printJobName.ToString().Split(',')[0] : "";
            string jobID = printJobId != null ? printJobId.ToString() : "";
            int totalPages = printJobTotalPages != null ? Convert.ToInt32(printJobTotalPages) : 0;

            Singleton singleton = Singleton.GetInstance();
            ManagementBaseObject job = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            uint jobId = job["JobID"] != null ? Convert.ToUInt32(job["JobID"]) : 0;
            var jobName = job["Name"];
            string printerName = jobName != null ? jobName.ToString().Split(',')[0] : "";

            BanInViewModel nV_BanIn = GetInfoSpooling(jobId, printerName, totalPages);

            nV_BanIn.PrintJob = jobID;
            nV_BanIn.Document = document;
            nV_BanIn.PaperSize = paperSize;
            nV_BanIn.TenMayIn = name;

            singleton.ListSpooling.Add(nV_BanIn);
        }

        public string GetIP()
        {
            string Ip4Address = "";
            ManagementObjectSearcher NetworkSearcher = new ManagementObjectSearcher("SELECT IPAddress FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = 'TRUE'");
            ManagementObjectCollection collectNetWork = NetworkSearcher.Get();
            foreach (ManagementObject obj in collectNetWork)
            {
                string[] arrIPAddress = (string[])(obj["IPAddress"]);

                return arrIPAddress[0];
            }
            return Ip4Address;
        }
        public void LogPrintServiceJob(object sender, EventRecordWrittenEventArgs e)
        {
            if (e.EventRecord != null)
            {
                if (e.EventRecord.Id == 307)
                {
                    Singleton singleton = Singleton.GetInstance();
                    string xml = e.EventRecord.ToXml();
                    XDocument doc = XDocument.Parse(xml);
                    XNamespace ns = "http://manifests.microsoft.com/win/2005/08/windows/printing/spooler/core/events";

                    var renderJobDiagNodes = doc.Descendants(ns + "DocumentPrinted");
                    string jobID = "";
                    foreach (var node in renderJobDiagNodes)
                    {
                        jobID = node.Element(ns + "Param1").Value;
                    }

                    BanInViewModel banInViewModel = singleton.ListSpooling.Where(_ => _.PrintJob == jobID).FirstOrDefault();
                    NV_BanIn banIn = null;

                    if (banInViewModel != null)
                    {
                        banIn = new NV_BanIn
                        {
                            TenTaiLieuDinhKem = banInViewModel.Document,
                            ThoiGianPrint = DateTime.Now,
                            TongSoTrangDaIn = banInViewModel.TongSoTrangDaIn,
                            PaperSize = banInViewModel.PaperSize,
                            TenMayIn = banInViewModel.TenMayIn,
                            TrangThaiText = "Đã In Thành Công",
                        };
                    }

                    if (
                    banIn.TenMayIn.ToLower() != "microsoft print to pdf" &&
                    banIn.TenMayIn.ToLower() != "fax" &&
                    banIn.TenMayIn.ToLower() != "microsoft xps document writer" &&
                    banIn.TenMayIn.ToLower() != "onenote for windows 10")
                    {
                        using (var context = new Print_LimitEntities())
                        {
                            string ip = GetIP();
                            //context.Database.Log = Console.Write;
                            var queryNV = context.DM_NhanVien.AsQueryable();
                            var queryBanIn = context.NV_BanIn.AsQueryable();


                            //Tìm kiếm nhân viên trong cache, có thì lấy địa chỉ IP, không thì truy vấn dưới DB.
                            var nhanVien = cachingService.GetFromCache<DM_NhanVien>(ID_NHAN_VIEN);
                            if (nhanVien == null)
                            {
                                nhanVien = queryNV.Where(_ => _.Bios_MayTinh == ip).FirstOrDefault();
                                cachingService.AddToCache(ID_NHAN_VIEN, nhanVien, DateTimeOffset.UtcNow.AddHours(1));
                            }
                            //Lấy ID nhân viên
                            banIn.ID_NhanVien = nhanVien?.ID_NhanVien;

                            context.NV_BanIn.Add(banIn);
                            context.SaveChanges();

                            singleton.ListSpooling.Remove(banInViewModel);
                            //singleton.Refresh();
                        }
                    }

                }
                else if (e.EventRecord.Id == 805)
                {
                    //string xml = e.EventRecord.ToXml();
                    //XDocument doc = XDocument.Parse(xml);
                    //XNamespace ns = "http://manifests.microsoft.com/win/2005/08/windows/printing/spooler/core/events";

                    //var renderJobDiagNodes = doc.Descendants(ns + "RenderJobDiag");

                    //foreach (var node in renderJobDiagNodes)
                    //{
                    //    var copiesAsString = node.Element(ns + "Copies").Value;
                    //    int copies = copiesAsString != "" ? Convert.ToInt32(copiesAsString) : 0;
                    //    Singleton singleton = Singleton.GetInstance();
                    //    singleton.Copies = copies;
                    //}
                }
            }
        }

        public void PrintJob(object sender, EventArrivedEventArgs e)
        {
            var printJobDocument = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Document"];
            var printJobPaperSize = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["PaperSize"];
            var printJobName = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"];
            var printJobId = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["JobId"];

            var singleton = Singleton.GetInstance();

            string document = printJobDocument != null ? printJobDocument.ToString() : "";
            string paperSize = printJobPaperSize != null ? printJobPaperSize.ToString() : "";
            string name = printJobName != null ? printJobName.ToString().Split(',')[0] : "";


            singleton.PaperSize = paperSize;
            singleton.Document = document;
            singleton.NamePrinter = name;

        }

        public BanInViewModel GetInfoSpooling(uint jobId, string printerName,int totalPages)
        {
            BanInViewModel nV_BanIn = new BanInViewModel();
            IntPtr hPrinter;

            if (!WinpoolDLL.OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
                throw new Exception("Failed to open printer.");

            uint bytesNeeded;
            WinpoolDLL.GetJob(hPrinter, jobId, 2, IntPtr.Zero, 0, out bytesNeeded);

            if (Marshal.GetLastWin32Error() != 122)  // ERROR_INSUFFICIENT_BUFFER
                throw new Exception("Failed to get job info.");

            IntPtr pJobInfo = Marshal.AllocHGlobal((int)bytesNeeded);

            if (WinpoolDLL.GetJob(hPrinter, jobId, 2, pJobInfo, bytesNeeded, out bytesNeeded))
            {
                JOB_INFO_2 jobInfo = (JOB_INFO_2)Marshal.PtrToStructure(pJobInfo, typeof(JOB_INFO_2));
                DEVMODE devMode = (DEVMODE)Marshal.PtrToStructure(jobInfo.pDevMode, typeof(DEVMODE));

                // Do what you need with devMode...
                switch (devMode.dmDuplex)
                {
                    case 1:
                        nV_BanIn.TongSoTrangDaIn = (int)totalPages * devMode.dmCopies;
                        break;
                    case 2:
                    case 3:
                        nV_BanIn.TongSoTrangDaIn = (int)Math.Round((double)totalPages / 2, MidpointRounding.ToEven) * devMode.dmCopies;
                        break;
                }
                Marshal.FreeHGlobal(pJobInfo);
            }
            else
            {
                throw new Exception("Failed to get job info.");
            }

            WinpoolDLL.ClosePrinter(hPrinter);

            return nV_BanIn;
        }
    }
}
