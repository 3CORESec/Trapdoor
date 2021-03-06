using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Trapdoor
{
    public class Function
    {
        private readonly IMemoryCache memoryCache;
        private List<ISender> _alerts;
        private readonly Storage<SessionLog> _storage;

        private Config config;
        public Function()
        {
            memoryCache = new MemoryCache(new MemoryCacheOptions());
            _storage = new Storage<SessionLog>(new AmazonDynamoDBClient());
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request)
        {
            initConfig();
            var sender = new Handler(config, memoryCache, _alerts);
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

        private void initConfig()
        {
            _alerts = new List<ISender>();
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            if (string.IsNullOrEmpty(config.SlackPath))
                config.SlackPath = Environment.GetEnvironmentVariable("SLACKPATH");
            if (string.IsNullOrEmpty(config.WebhookChannel))
                config.WebhookChannel = Environment.GetEnvironmentVariable("WEBHOOKCHANNEL");
            if (string.IsNullOrEmpty(config.WebHookToken))
                config.WebHookToken = Environment.GetEnvironmentVariable("WEBHOOKTOKEN");
            if (string.IsNullOrEmpty(config.PostUrl))
                config.PostUrl = Environment.GetEnvironmentVariable("POSTURL");
            var type = typeof(ISender);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
            types.ToList().ForEach(type => {
                ConstructorInfo ctor = type.GetConstructor(new[] { typeof(Storage<SessionLog>), typeof(Config), typeof(IMemoryCache) });
                ISender instance = ctor.Invoke(new object[] { _storage, config, memoryCache }) as ISender;
                _alerts.Add(instance);
            });
        }
    }
}