using PrintLimit.Data.EF;
using PrintLimit.Services.CachingServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.DALServices
{
    class DALService : IDALService
    {
        DM_NhanVien IDALService.CheckEmployeeViaIP(string ip)
        {
            using (var context = new Print_LimitEntities())
            {
                return context.DM_NhanVien.Where(_ => _.Bios_MayTinh == ip).FirstOrDefault();
            }
        }
    }
}
