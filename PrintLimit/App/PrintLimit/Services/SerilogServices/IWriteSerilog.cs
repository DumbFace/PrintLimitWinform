using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.SerilogServices
{
    interface IWriteSerilog
    {
        void WriteLogInfo(string name, string message);
        void WriteLogHeader(string name);
        void BreakDownLine();
        void WriteBlockLog(string header = "",List<KeyValuePair<string,string>> lst = null);
        void PrintProperties<T>(string name,T obj);
    }
}
