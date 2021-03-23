using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Trapdoor
{
    public abstract class SenderBase : ISender
    {
        private readonly Storage<SessionLog> _storage;
        private readonly IMemoryCache memoryCache;

        protected async Task<List<string>> GetLogs(string id)
        {
            string type = this.GetType().Name;
            return (await _storage.GetByHash($"{type}#{id}"))
                .Select(x => Compression.Unzip(x.Data)).Distinct().ToList();
        }

        public async Task StoreLogs(string id, string data)
        {
            string type = this.GetType().Name;
            try
            {
                await _storage.Store(new SessionLog()
                {
                    Id = $"{type}#{id}",
                    Data = Compression.Zip(data)

                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error storing logs : {e.Message}");
            }
        }

        protected SenderBase(Storage<SessionLog> storage, IMemoryCache cache)
        {
            _storage = storage;
            memoryCache = cache;
        }

        public abstract Task<string> SendAlert((string, Dictionary<string, dynamic>) res, string sourceIp, string path, string guid);
    }
}
