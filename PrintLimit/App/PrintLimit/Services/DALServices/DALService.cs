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
                 DM_NhanVien nhanVien = context.DM_NhanVien.Where(_ => _.Bios_MayTinh == ip).FirstOrDefault();
                //if (nhanVien != null)
                //{
                //    return nhanVien;
                //}
                //else
                //{
                //    cachingService.AddToCache(ID_NHAN_VIEN, nhanVien.ID_NhanVien, DateTimeOffset.UtcNow.AddMinutes(5));
                //}
                return nhanVien;
            }
        }
    }
}
