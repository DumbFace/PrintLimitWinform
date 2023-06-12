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
            Log.Information("Tạo Service", process.StartInfo);
            bool result = process.Start();
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
            Log.Information("Chạy Service", process.StartInfo);
            bool result = process.Start();

        }
    }
}
