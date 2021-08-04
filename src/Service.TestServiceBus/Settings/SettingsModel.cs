using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.TestServiceBus.Settings
{
    public class SettingsModel
    {
        [YamlProperty("TestServiceBus.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("TestServiceBus.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("TestServiceBus.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("TestServiceBus.ServiceBusHostPort")]
        public string ServiceBusHostPort { get; set; }

        [YamlProperty("TestServiceBus.WebServiceBusHostPort")]
        public string WebServiceBusHostPort { get; set; }

        [YamlProperty("TestServiceBus.Name")]
        public string Name { get; set; }

        [YamlProperty("TestServiceBus.ChatId")]
        public long ChatId { get; set; }

        [YamlProperty("TestServiceBus.BotApiKey")]
        public string BotApiKey { get; set; }

        
    }
}
