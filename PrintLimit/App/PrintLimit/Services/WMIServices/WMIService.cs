using PrintLimit.Data.EF;
using PrintLimit.Services.CachingServices;
using PrintLimit.Services.DALServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PrintLimit.Services.WMIServices
{
    class WMIService : IWMIService
    {
        private System.Timers.Timer aTimer = new System.Timers.Timer();
        const string ID_NHAN_VIEN = "idNhanVien";
        private readonly ICachingService cachingService;
        public WMIService()
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

                Ip4Address = arrIPAddress[0];
            }
            return Ip4Address;
        }


        private bool enablePrint = true;
        public void GetPrintJob(object sender, EventArrivedEventArgs e)
        {
            string ip = GetIP();
            NV_BanIn banIn = null;
            var JobStatus = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["JobStatus"];
            var Status = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Status"].ToString();

            JobStatus = JobStatus != null ? JobStatus.ToString() : "";

            if (JobStatus.ToString() == "Printing" && Status == "OK" && enablePrint)
            {
                banIn = GetBanInViaPrintJob(e);
                if (
                banIn.TenMayIn.ToLower() != "microsoft print to pdf" &&
                banIn.TenMayIn.ToLower() != "fax" &&
                banIn.TenMayIn.ToLower() != "microsoft xps document writer" &&
                banIn.TenMayIn.ToLower() != "onenote for windows 10")
                {
                    using (var context = new Print_LimitEntities())
                    {
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

                        if (enablePrint)
                        {
                            banIn.ThoiGianPrint = DateTime.Now;
                            banIn.ThoiGianUpload = DateTime.Now;
                            context.NV_BanIn.Add(banIn);
                            context.SaveChanges();
                            enablePrint = false;
                        }

                        aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                        aTimer.Interval = 10000;
                        aTimer.Enabled = true;
                    }
                }
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            // Update the label in the UI thread
            enablePrint = true;
            aTimer.Enabled = false;
        }


        public NV_BanIn GetBanInViaPrintJob(EventArrivedEventArgs e)
        {
            var DriverName = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["DriverName"].ToString();
            var Document = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Document"].ToString();
            var Name = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"].ToString();
            var JobId = Int32.Parse(((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["JobId"].ToString());
            var PaperSize = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["PaperSize"].ToString();
            var TotalPages = Int32.Parse(((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["TotalPages"].ToString());

            var TenMayIn = Name.Split(',')[0].ToString().ToLower();
            var SoMayIn = DriverName.ToString().ToLower();
            NV_BanIn banIn = new NV_BanIn
            {
                TenTaiLieuDinhKem = Document,
                TrangThai = true,
                TrangThaiText = "Đã In Thành Công",
                TenMayIn = Name.Split(',')[0].ToLower(),
                TongSoTrang = TotalPages,
                TongSoTrangDaIn = TotalPages,
                JobID = JobId,
                PaperSize = PaperSize
            };

            return banIn;
        }
    }
}
