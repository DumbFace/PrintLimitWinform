using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Models
{
    public class BanInViewModel
    {
        public string PrintJob { get; set; }
        public string Document { get; set; }
        public int TongSoTrangDaIn { get; set; }
        public string PaperSize { get; set; }
        public string TenMayIn { get; set; }
    }
}
