using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PrintLimit.Services.SerilogServices
{
    class SerilogService : IWriteSerilog
    {
        public SerilogService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string vinaAiPath = Path.Combine(appDataPath, "VinaAi\\Logs\\");


            // Kiểm tra xem thư mục VinaAi có tồn tại không, nếu không thì tạo mới
            if (!Directory.Exists(vinaAiPath))
            {
                Directory.CreateDirectory(vinaAiPath);
            }

            Log.Logger = new LoggerConfiguration()
                  .MinimumLevel.Debug()
                  .WriteTo.Console()
                  .WriteTo.File(vinaAiPath, rollingInterval: RollingInterval.Day)
                  .CreateLogger();
        }

        public void WriteLogHeader(string name)
        {
            Log.Information($"----------{name}----------");
        }

        public void WriteLogInfo(string name, string message)
        {
            Log.Information($"{name}: {message}");
        }

        public void BreakDownLine()
        {
            Log.Information($"\n\n");
        }

        public void WriteBlockLog(string header = "", List<KeyValuePair<string, string>> lst = null)
        {
            WriteLogHeader(header);
            foreach (var item in lst)
            {
                Log.Information($"{item.Key}: {item.Value}");
            }
            Log.Information($"\n\n");
        }
    }
}
