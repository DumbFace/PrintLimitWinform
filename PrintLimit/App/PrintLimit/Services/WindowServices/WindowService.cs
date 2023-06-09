using PrintLimit.Services.SerilogServices;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.WindowServices
{
    class WindowService : IWindowService
    {
        private readonly IWriteSerilog writeSerilog;
        public WindowService()
        {
            writeSerilog = new SerilogService();
        }

        public void CreateService()
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string exeDirectory = System.IO.Path.GetDirectoryName(exePath);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"create PrintLimit binPath={exePath} start= auto",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            writeSerilog.PrintProperties("Tạo Service", process.StartInfo);
            bool result = process.Start();
            if (result)
            {
                Log.Information("Tạo service thành công!");
            } 
            else
            {
                Log.Information("Tạo service không thành công!");
            }

        }

        public void StartService()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = "start PrintLimit",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            writeSerilog.PrintProperties("Chạy Service", process.StartInfo);
            bool result = process.Start();

            if (result)
            {
                Log.Information("Chạy service thành công!");
            }
            else
            {
                Log.Information("Chạy service không thành công!");
            }
        }
    }
}
