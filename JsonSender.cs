using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Trapdoor
{
    public class JsonSender : SenderBase
    {

        private readonly string send_link;
        private readonly HttpClient _client;
        private readonly Storage<SessionLog> _storage;
        private readonly IMemoryCache memoryCache;
        private readonly Dictionary<string, string> paths;

        public JsonSender(Storage<SessionLog> storage, Config config, IMemoryCache cache) : base(storage, config, cache)
        {
            _storage = storage;
            send_link = config.WebHookPost;
            _client = new HttpClient();
            paths = config.Paths;
            memoryCache = cache;
        }

        public override async Task<string> SendAlert((string, Dictionary<string, dynamic>) res, string sourceIp, string path, string guid)
        {
            try
            {
                var _path = paths.ContainsKey(path) ? paths[path] : path;
                var message = $"Trapdoor triggered in: {_path}";
                var temp = await GenerateAlert(res, sourceIp);
                var content = new StringContent(temp, Encoding.UTF8, "application/json");
                await _client.PostAsync(send_link, content);
                return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private async Task<string> GenerateAlert((string, Dictionary<string, dynamic>) res, string sourceIp)
        {
            var ipLinks = new List<string>();
            var sessionLinks = new List<string>();
            try
            {
                if (!string.IsNullOrEmpty(res.Item1))
                {
                    var sessionLogs = await GetLogs(res.Item1);
                    if (sessionLogs.Any())
                    {
                        res.Item2["Session ID Hits"] = sessionLogs;
                    }
                }

                var ipLogs = await GetLogs(sourceIp);

                if (ipLogs.Any())
                {
                    res.Item2["IP Hits"] = ipLogs;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting logs : {e.Message}");
            }

            return JsonSerializer.Serialize(res.Item2);
        }
    }
}
