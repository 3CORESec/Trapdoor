using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace Trapdoor
{
    public class Handler
    {
        private readonly HttpClient _client;
        private readonly string tor_exit_list;
        private readonly Storage<SessionLog> _storage;
        private readonly List<ISender> _alerts;
        public Handler(Config config, IMemoryCache memoryCache, List<ISender> alerts)
        {
            _alerts = alerts;
            tor_exit_list = config.TorExitList;
            _client = new HttpClient();
            //_storage = new Storage<SessionLog>(new AmazonDynamoDBClient());
            //_alerts.Add(new SlackSender(_storage, config, memoryCache));
        }
        public async Task<bool> SendAlerts(APIGatewayProxyRequest request, string guid)
        {
            var fields = await ParseAlert(request);
            foreach (var alert in _alerts)
            {
                try
                {
                    var sourceIp = request.RequestContext.Identity.SourceIp;
                    var res = await alert.SendAlert(fields, sourceIp, request.Path, guid);
                    if (fields.Item1 != null)
                        await alert.StoreLogs(fields.Item1, res);
                    else
                        await alert.StoreLogs(sourceIp, res);
                }
                catch (Exception e){
                    Console.WriteLine($"Error in {alert.GetType().Name}: {e.Message}");
                }
            }

            return fields.Item1 == null;
        }

        private async Task<(string, Dictionary<string, dynamic>)> ParseAlert(APIGatewayProxyRequest request)
        {
            Dictionary<string, dynamic> alert = new Dictionary<string, dynamic>();
            string id = null;

            if (request.Body != null)
            {
                var collection = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Body);
                foreach (string key in collection.Keys)
                {
                    alert[key] = collection[key];
                    if (key == "Session ID")
                        id = collection["Session ID"];
                }
            }
            alert["Path"] = request.Path;
            alert["Full Path"] = request.RequestContext.Path;
            alert["Host"] = request.Headers["Host"];
            alert["HTTP Method"] = request.HttpMethod;
            alert["User Agent"] = request.Headers["User-Agent"];
            if (request.Headers.ContainsKey("CloudFront-Viewer-Country"))
                alert["Viewer Country"] = request.Headers["CloudFront-Viewer-Country"];
            else
                alert["Viewer Country"] = "None";
            if (request.Headers["CloudFront-Is-Tablet-Viewer"] == "true")
                alert["Viewer Device"] = "Tablet";
            else if (request.Headers["CloudFront-Is-Mobile-Viewer"] == "true")
                alert["Viewer Device"] = "Mobile";
            else if (request.Headers["CloudFront-Is-Desktop-Viewer"] == "true")
                alert["Viewer Device"] = "Desktop";
            else if (request.Headers["CloudFront-Is-SmartTV-Viewer"] == "true")
                alert["Viewer Device"] = "SmartTV";
            alert["Tor Network"] = await TorExitUsed(request);
            alert["Source IP"] = request.RequestContext.Identity.SourceIp;
            return (id, alert);
        }

        private async Task<string> TorExitUsed(APIGatewayProxyRequest request)
        {
            try
            {
                var result = await _client.GetAsync(tor_exit_list);
                var stream = await result.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (request.RequestContext.Identity.SourceIp == line)
                        return "true";

                }

                return "false";
            }
            catch (Exception)
            {
                return "false";
            }
        }

    }
}