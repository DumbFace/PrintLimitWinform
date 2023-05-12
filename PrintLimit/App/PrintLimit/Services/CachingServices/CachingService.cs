using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit.Services.CachingServices
{
    class CachingService : ICachingService
    {
        private static MemoryCache cache = new MemoryCache("CachingProvider");

        public void AddToCache(string key, object value, DateTimeOffset absExpiration)
        {
            cache.Add(key, value, absExpiration);
        }

        public T GetFromCache<T>(string key) where T : class
        {
            try
            {
                return (T)cache[key];
            }
            catch
            {
                return null;
            }
        }
    }
}
