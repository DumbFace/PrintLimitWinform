using PrintLimit.Data.EF;
using PrintLimit.Services.CachingServices;
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
                    int totalPages = 0;
                    foreach (var node in renderJobDiagNodes)
                    {
                        var totalPagesAsString = node.Element(ns + "Param8").Value;
                        totalPages = totalPagesAsString != "" ? Convert.ToInt32(totalPagesAsString) : 0;
                    }

                    NV_BanIn banIn = new NV_BanIn
                    {
                        TenTaiLieuDinhKem = singleton.Document,
                        ThoiGianPrint = DateTime.Now,
                        TongSoTrangDaIn = singleton.Copies * totalPages,
                        PaperSize = singleton.PaperSize,
                        TenMayIn = singleton.NamePrinter,
                        TrangThaiText = "Đã In Thành Công",
                    };

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

                            singleton.Refresh();
                        }
                    }

                }
                else if (e.EventRecord.Id == 805)
                {
                    string xml = e.EventRecord.ToXml();
                    XDocument doc = XDocument.Parse(xml);
                    XNamespace ns = "http://manifests.microsoft.com/win/2005/08/windows/printing/spooler/core/events";

                    var renderJobDiagNodes = doc.Descendants(ns + "RenderJobDiag");

                    foreach (var node in renderJobDiagNodes)
                    {
                        var copiesAsString = node.Element(ns + "Copies").Value;
                        int copies = copiesAsString != "" ? Convert.ToInt32(copiesAsString) : 0;
                        Singleton singleton = Singleton.GetInstance();
                        singleton.Copies = copies;
                    }
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

            string printerName = name; // Replace with your printer's name
            PrintServer printServer = new PrintServer();
            PrintQueue printQueue = printServer.GetPrintQueue(printerName);

            printQueue.Refresh();

            foreach (PrintSystemJobInfo job in printQueue.GetPrintJobInfoCollection())
            {
                var defaultPrintTicket = job.HostingPrintQueue.DefaultPrintTicket.Duplexing;
                Console.WriteLine(job.HostingPrintQueue.UserPrintTicket.Duplexing);
                //Console.WriteLine("Job ID: {0}", job.HostingPrintServer.Dup);
                //Console.WriteLine("Copies: {0}", job.NumberOfPages);
            }


           
        }
    }
}
