using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.CachingServices
{
    interface ICachingService
    {
        void AddToCache(string key, object value, DateTimeOffset absExpiration);
        T GetFromCache<T>(string key) where T : class;
    }
}
