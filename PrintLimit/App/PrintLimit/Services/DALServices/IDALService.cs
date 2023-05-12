using PrintLimit.Data.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.DALServices
{
    interface IDALService
    {
        DM_NhanVien CheckEmployeeViaIP(string ip);
    }
}
