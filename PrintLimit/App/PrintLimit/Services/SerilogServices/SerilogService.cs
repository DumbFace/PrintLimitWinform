using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            string vinaAiPath = appDataPath + "VinaAi\\Logs\\";
            //string vinaAiPath = "C:\\";

            // Kiểm tra xem thư mục VinaAi có tồn tại không, nếu không thì tạo mới
            if (!Directory.Exists(vinaAiPath))
            {
                Directory.CreateDirectory(vinaAiPath);
            }

            Log.Logger = new LoggerConfiguration()
                  .MinimumLevel.Information()
                  .WriteTo.File(Environment.GetEnvironmentVariable("APPDATA") + "/serilog/logs/", rollingInterval: RollingInterval.Day)
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

        public void PrintProperties<T>(string name, T obj)
        {
            WriteLogHeader(name);

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();

            foreach (var property in properties)
            {
                Log.Information($"{property.Name}: {property.GetValue(obj)}");
            }
        }
    }
}
