using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Models
{
    class PrintJobModel
    {
        public int Copies { get; set; } = 1;
        public bool Duplex { get; set; } = false;
        public int TotalPages { get; set; } = 1;
        public string PaperSize { get; set; } = "";
    }
}
