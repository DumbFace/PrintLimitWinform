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
using Serilog;
using System.Windows.Forms;
using System.IO;
using PrintLimit.Services.AnalysisSpoolServices;
using System.IO.Compression;
using System.Threading;
using Serilog.Core;
using PrintLimit.Services.SerilogServices;

namespace PrintLimit.Services.JobServices
{
    class JobService : IJobService
    {

        const string ID_NHAN_VIEN = "idNhanVien";
        private readonly ICachingService cachingService;
        private readonly IAnalysisSpoolService analysisSpoolService;
        private readonly IWriteSerilog serilogService;
        public JobService()
        {
            analysisSpoolService = new AnalysisSpoolService();
            cachingService = new CachingService();
            serilogService = new SerilogService();
        }

        public void MonitorSpoolingJob(object sender, EventArrivedEventArgs e)
        {
            try
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

                BanInViewModel nV_BanIn = new BanInViewModel();
                nV_BanIn.PrintJob = jobID;
                nV_BanIn.Document = document;
                nV_BanIn.PaperSize = paperSize;
                nV_BanIn.TenMayIn = name;


                singleton.ListSpooling.Add(nV_BanIn);

                serilogService.PrintProperties("Bản in từ Print Monitor", nV_BanIn);
            }
            catch (Exception ex)
            {
                Log.Error($"Có lỗi xảy ra trong quá trình giám sát win32 print job: {ex.Message}");
            }

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
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string vinaAiPath = Path.Combine(appDataPath, "VinaAi\\");

            if (e.EventRecord != null)
            {
                if (e.EventRecord.Id == 307)
                {
                    try
                    {
                        string xml = e.EventRecord.ToXml();
                        XDocument doc = XDocument.Parse(xml);
                        XNamespace ns = "http://manifests.microsoft.com/win/2005/08/windows/printing/spooler/core/events";

                        var renderJobDiagNodes = doc.Descendants(ns + "DocumentPrinted");
                        string jobID = "";
                        string tenMayIn = "";
                        string userEventViewer = "";
                        string guestIP = "";
                        foreach (var node in renderJobDiagNodes)
                        {
                            jobID = node.Element(ns + "Param1").Value;
                            userEventViewer = node.Element(ns + "Param3").Value;
                            guestIP = node.Element(ns + "Param4").Value.Replace("\\", "");
                            //tenMayIn = node.Element(ns + "Param5").Value;
                        }

                        Singleton singleton = Singleton.GetInstance();
                        BanInViewModel banInViewModel = singleton.ListSpooling.Where(_ => _.PrintJob == jobID).FirstOrDefault();

                        if (banInViewModel == null)
                        {
                            banInViewModel = new BanInViewModel();
                            banInViewModel.Document = "Null";
                            banInViewModel.TenMayIn = "Null";
                        }

                        string filePath = vinaAiPath + $"{jobID.PadLeft(5, '0')}.SPL";
                        using (ZipArchive archive = ZipFile.OpenRead(filePath))
                        {

                            serilogService.WriteLogInfo($"Đọc File", filePath);
                            PrintJobModel printJobModel = analysisSpoolService.GetCopyDuplexPaperSize(archive);
                            printJobModel.TotalPages = analysisSpoolService.GetTotalPages(archive);

                            serilogService.PrintProperties("Analysis", printJobModel);

                            banInViewModel.Duplex = printJobModel.Duplex;
                            banInViewModel.Copies = printJobModel.Copies;
                            banInViewModel.TotalPagesPerDoc = printJobModel.TotalPages;
                            banInViewModel.PaperSize = printJobModel.PaperSize;
                        }

                        NV_BanIn banIn = null;
                        if (banInViewModel != null)
                        {
                            banIn = new NV_BanIn
                            {
                                TenTaiLieuDinhKem = banInViewModel.Document,
                                ThoiGianPrint = DateTime.Now,
                                TongSoTrangDaIn = banInViewModel.Duplex ?
                                                            (int)Math.Round((double)banInViewModel.TotalPagesPerDoc / 2, MidpointRounding.AwayFromZero) *
                                                            banInViewModel.Copies :
                                                            (int)banInViewModel.TotalPagesPerDoc * banInViewModel.Copies,
                                PaperSize = banInViewModel.PaperSize,
                                TenMayIn = banInViewModel.TenMayIn,
                                TrangThaiText = "Đã In Thành Công",
                            };
                        }

                        if (banIn != null)
                        {
                            if (
                                banIn.TenMayIn.ToLower() != "microsoft print to pdf" &&
                                banIn.TenMayIn.ToLower() != "fax" &&
                                banIn.TenMayIn.ToLower() != "microsoft xps document writer" &&
                                banIn.TenMayIn.ToLower() != "onenote for windows 10")
                            {
                                using (var context = new Print_LimitEntities())
                                {
                                    string ip = GetIP();
                                    var queryNV = context.DM_NhanVien.AsQueryable();
                                    var queryBanIn = context.NV_BanIn.AsQueryable();

                                    //User đăng nhập hiện tại
                                    string currentUser = Environment.UserName;

                                    //Nếu user đăng nhập hiện tại khác với user trong event viewer sẽ lấy IP của máy trạm ngược lại thì máy share.
                                    ip = currentUser == userEventViewer ? ip : guestIP;

                                    banIn.ID_NhanVien = queryNV.Where(_ => _.Bios_MayTinh == ip).FirstOrDefault().ID_NhanVien;

                                    context.NV_BanIn.Add(banIn);
                                    context.SaveChanges();

                                    singleton.ListSpooling.Remove(banInViewModel);

                                    serilogService.PrintProperties("Thêm bản in thành công", banIn);
                                    serilogService.WriteLogInfo("IP Máy tính", ip);
                                }
                            }
                        }
                        else
                        {
                            Log.Error("Bản In Null");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Có lỗi trong quá trình thêm dữ liệu vào DB: {ex.Message}");
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
        }

        public void OnSpoolingCreated(object source, FileSystemEventArgs e)
        {

            serilogService.WriteLogHeader("Giám sát sự kiện SPL file");
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string vinaAiPath = Path.Combine(appDataPath, "VinaAi\\");

            // Kiểm tra xem thư mục VinaAi có tồn tại không, nếu không thì tạo mới
            if (!Directory.Exists(vinaAiPath))
            {
                Directory.CreateDirectory(vinaAiPath);
            }
            string numberAsString = e.Name.Split('.')[0];

            if (int.TryParse(numberAsString, out _))
            {
                try
                {
                    byte[] fileBytes = File.ReadAllBytes(e.FullPath);
                    File.WriteAllBytes(vinaAiPath + Path.GetFileName(e.FullPath), fileBytes);
                    Log.Information($"File đã ghi vào app {vinaAiPath + Path.GetFileName(e.FullPath)}");
                }
                catch (Exception ex)
                {
                    Log.Error($"Có lỗi xảy trong trong quá trình copy files SPL: {ex.Message}");
                    Log.Error($"Đường dẫn file {e.FullPath}");
                }
            }
        }
    }
}
