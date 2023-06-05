using PrintLimit.Models;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.AnalysisSpoolServices
{
    interface IAnalysisSpoolService
    {
        int GetTotalPages(ZipArchive archive);
        PrintJobModel GetCopyDuplexPaperSize(ZipArchive archive);
    }
}
