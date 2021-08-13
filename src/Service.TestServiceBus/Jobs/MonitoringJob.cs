using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core.Logging;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Telegram.Bot;

namespace Service.TestServiceBus.Jobs
{
    public class MonitoringJob
    {
        private readonly MyServiceBusTcpClient _client;

        private Dictionary<string, long> _lasted = new Dictionary<string, long>();
        private TelegramBotClient _botApiClient;

        public MonitoringJob(MyServiceBusTcpClient client)
        {
            _client = client;
        }

        public void Start()
        {
            if (Program.Settings.ChatId == 0)
            {
                return;
            }

            var sb = new StringBuilder();

            var url = $"http://{Program.Settings.WebServiceBusHostPort}/Topics";

            var http = new HttpClient();

            var body = http.GetStringAsync(url).GetAwaiter().GetResult();

            var list = JsonSerializer.Deserialize<List<TopicItem>>(body);

            foreach (var item in list)
            {
                SubscribeToTopic(item.id, sb);
            }

            _botApiClient = new TelegramBotClient(Program.Settings.BotApiKey);

            _botApiClient.SendTextMessageAsync(Program.Settings.ChatId, sb.ToString()).GetAwaiter().GetResult();
        }

        private void SubscribeToTopic(string topic, StringBuilder sb)
        {
            _lasted[$"{topic}[single]"] = 0;
            _lasted[$"{topic}[batch]"] = 0;
            
            _client.Subscribe(topic, "tester-single-1", TopicQueueType.DeleteOnDisconnect, m => BidAskCallback(m, $"{topic}[single]"));
            
            _client.Subscribe(topic, "tester-batch-1", TopicQueueType.DeleteOnDisconnect, (context, list) => BidAskBatchCallback(list, $"{topic}[batch]"));

            sb.AppendLine($"[{Program.Settings.Name}] Subscribe to {topic}");
        }

        private async ValueTask BidAskCallback(IMyServiceBusMessage message, string topic)
        {
            long lasted;

            lock(_lasted) lasted = _lasted[topic];
            
            if (lasted > 0 && lasted != message.Id - 1)
            {
                Console.WriteLine($"Wrong ID, Topic {topic}. Receive Id = {message.Id}, but lasted = {lasted}");
                await _botApiClient.SendTextMessageAsync(Program.Settings.ChatId, $"{Program.Settings.Name}] Wrong ID, Topic {topic}. Receive Id = {message.Id}, but lasted = {lasted}");
            }

            lock (_lasted) _lasted[topic] = message.Id;
        }

        private async ValueTask BidAskBatchCallback(IReadOnlyList<IMyServiceBusMessage> messages, string topic)
        {
            if (messages.Count == 0)
                return;

            long lasted;
            lock (_lasted) lasted = _lasted[topic];

            var min = messages.Min(e => e.Id);
            var max = messages.Max(e => e.Id);

            if (lasted > 0 && lasted != min - 1)
            {
                Console.WriteLine($"Wrong ID, Topic {topic}. Receive min Id = {min}, max = {max}, but lasted = {lasted}");
                await _botApiClient.SendTextMessageAsync(Program.Settings.ChatId, $"[{Program.Settings.Name}] Wrong ID, Topic {topic}. Receive min Id = {min}, max = {max}, but lasted = {lasted}");
            }

            if (messages.Count != max - min + 1)
            {
                Console.WriteLine($"Miss messages in batch, Topic {topic}. Receive min Id = {min}, max = {max}, but count = {messages.Count}");
                await _botApiClient.SendTextMessageAsync(Program.Settings.ChatId, $"[{Program.Settings.Name}] Miss messages in batch, Topic {topic}. Receive min Id = {min}, max = {max}, but count = {messages.Count}");
            }

            lock (_lasted) _lasted[topic] = max;

            //Console.WriteLine($"{topic} - {messages.Count}");
        }

        public class TopicItem
        {
            public string id { get; set; }
            public long messageId { get; set; }
        }
    }
}