using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Trapdoor
{
    public class SlackSender : SenderBase
    {
        private readonly string country_flag_link;
        private readonly string threat_intel_link;
        private readonly string country_code_link;
        private readonly HttpClient _client;
        private readonly Storage<SessionLog> _storage;
        private readonly IMemoryCache memoryCache;
        private readonly Dictionary<string, string> paths;
        private readonly Slack _sender;

        public SlackSender(Storage<SessionLog> storage, Config config, IMemoryCache cache) : base(storage, cache)
        {
            _storage = storage;
            _sender = new Slack(config);
            country_flag_link = config.FlagIcon;
            threat_intel_link = config.ThreatIntelLookup;
            country_code_link = config.IpLookup;
            paths = config.Paths;
            _client = new HttpClient();
            memoryCache = cache;
        }

        private async Task<(string, string)> GenerateAlert((string, Dictionary<string, dynamic>) res, string sourceIp)
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
                        sessionLinks = sessionLogs;
                        res.Item2["Session Id Hits"] = sessionLogs.Count;
                    }
                }

                var ipLogs = await GetLogs(sourceIp);

                if (ipLogs.Any())
                {
                    ipLinks = ipLogs;
                    res.Item2["Ip Hits"] = ipLogs.Count;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting logs : {e.Message}");
            }

            return (
                res.Item1,
                JsonSerializer.Serialize(new List<dynamic>
                {
                    new
                    {
                        title = sourceIp,
                        title_link = threat_intel_link.Replace("{IP}", sourceIp),
                        color = "danger",
                        text = "",
                        footer = "Trapdoor by 3CORESec",
                        thumb_url = country_flag_link.Replace("{CC}", await GetCountryCode(sourceIp)),
                        fields = res.Item2
                            .Select(x => new {title = x.Key, value = x.Value, @short = true})
                            .Concat(new List<dynamic>{new {title = "Previous Session Logs: "}})
                            .Concat(sessionLinks.Select(x => new { value = x}))
                            .Concat(new List<dynamic>{new {title = "Previous Ip Logs: "}})
                            .Concat(ipLinks.Select(x => new {value = x}))
                    }
                }));
        }
        private async Task<string> GetCountryCode(string sourceIp)
        {
            try
            {
                var result = await _client.GetAsync(country_code_link.Replace("{IP}", sourceIp));
                return (await result.Content.ReadAsStringAsync()).Replace("\n", "").ToLower();
            }
            catch (Exception)
            {
                return "";
            }
        }
        private async Task<string> SendAlert(string path, (string, Dictionary<string, dynamic>) res, string sourceIp, string ts = null)
        {
            var _path = paths.ContainsKey(path) ? paths[path] : path;
            var message = $"Trapdoor triggered in: {_path}";
            var temp = await GenerateAlert(res, sourceIp);
            if (!string.IsNullOrEmpty(ts))
                return _sender.EditNotification(temp.Item2, message, ts);
            return _sender.SendNotification(temp.Item2, message);
        }
        public override async Task<string> SendAlert((string, Dictionary<string, dynamic>) res, string sourceIp, string path, string guid)
        {
            string ts;
            try
            {
                if (memoryCache.TryGetValue(path, out var temp))
                {
                    ts = await SendAlert(path.Split("/")[1], res, sourceIp, temp.ToString());
                    return  _sender.GenerateSlackLink(ts);

                }

                ts = await SendAlert(path.Split("/")[1], res, sourceIp);
                memoryCache.Set(path + "/" + guid, ts, new TimeSpan(0, 1, 0));
                return _sender.GenerateSlackLink(ts);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}