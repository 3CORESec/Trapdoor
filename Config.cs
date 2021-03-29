using System.Collections.Generic;

namespace Trapdoor
{
    public class Config
    {
        public Dictionary<string, string> Paths { get; set; } 
        public string WebhookChannel { get; set; }
        public string SlackPath { get; set; }
        public string WebHookToken { get; set; }
        public string TorExitList { get; set; }
        public string IpLookup { get; set; }
        public string FlagIcon { get; set; }
        public string ThreatIntelLookup { get; set; }
        public string JsonLink { get; set; }
    }
}
