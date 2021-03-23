using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Trapdoor
{
    public class Function
    {
        private readonly IMemoryCache memoryCache;
        private Config config;
        public Function()
        {
            memoryCache = new MemoryCache(new MemoryCacheOptions());
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllTextAsync("config.json").Result);
            config.SlackPath = Environment.GetEnvironmentVariable("SLACKPATH");
            config.WebhookChannel = Environment.GetEnvironmentVariable("WEBHOOKCHANNEL");
            config.WebHookToken = Environment.GetEnvironmentVariable("WEBHOOKTOKEN");
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request)
        {
            var sender = new Handler(config, memoryCache);
            var guid = Guid.NewGuid().ToString();
            if (await sender.SendAlerts(request, guid))
                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Headers = new Dictionary<string, string> { { "Content-type", "text/html" } },
                    Body = (await File.ReadAllTextAsync("handle.html")).Replace("{REQUEST_ID}", guid)
                };
            return new APIGatewayProxyResponse()
            {
                StatusCode = 200
            };
        }
    }
}