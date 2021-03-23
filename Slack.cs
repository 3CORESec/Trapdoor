using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Trapdoor
{
    public class Slack
    {
        private readonly string channel;
        private readonly string slackPath;
        private readonly string token;
        private readonly WebClient client;
        public Slack(Config config)
        {
            channel = config.WebhookChannel;
            token = config.WebHookToken;
            slackPath = config.SlackPath;
            client = new WebClient();
        }
        public string EditNotification(string payload, string text, string ts)
        {
            var data = new NameValueCollection();
            data["token"] = token;
            data["channel"] = channel;
            data["as_user"] = "true";
            data["text"] = text;
            data["ts"] = ts;
            data["attachments"] = payload;
            var response = client.UploadValues("https://slack.com/api/chat.update", "POST", data);
            Console.WriteLine($"Editing:{Encoding.UTF8.GetString(response)}");
            return JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(response))["ts"];
        }
        public  string SendNotification(string payload, string text)
        {
            var data = new NameValueCollection();
            data["token"] = token;
            data["channel"] = channel;
            data["as_user"] = "true";
            data["text"] = text;
            data["attachments"] = payload;
            var response = client.UploadValues("https://slack.com/api/chat.postMessage", "POST", data);
            Console.WriteLine($"Sending:{Encoding.UTF8.GetString(response)}");
            return JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(response))["ts"];
        }

        public string GenerateSlackLink(string ts)
        {
            return $"{slackPath}/archives/{channel}/p{ts.Replace(".", "")}";
        }
    }
}
