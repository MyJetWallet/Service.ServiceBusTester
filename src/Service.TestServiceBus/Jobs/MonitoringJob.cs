using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var topic = "bidask";
            var sb = new StringBuilder();

            SubscribeToTopic(topic, sb);


            _botApiClient = new TelegramBotClient(Program.Settings.BotApiKey);

            _botApiClient.SendTextMessageAsync(Program.Settings.ChatId, sb.ToString()).GetAwaiter().GetResult();
        }

        private void SubscribeToTopic(string topic, StringBuilder sb)
        {
            _lasted[$"{topic}[single]"] = 0;
            _lasted[$"{topic}[batch]"] = 0;
            _client.Subscribe(topic, "tester-single", TopicQueueType.DeleteOnDisconnect,
                m => BidAskCallback(m, $"{topic}[single]"));
            _client.Subscribe(topic, "tester-batch", TopicQueueType.DeleteOnDisconnect,
                (context, list) => BidAskBatchCallback(list, $"{topic}[batch]"));
            sb.AppendLine($"[{Program.Settings.Name}] Subscribe to {topic}");
        }

        private ValueTask BidAskCallback(IMyServiceBusMessage message, string topic)
        {
            var lasted = _lasted[topic];
            if (lasted > 0 && lasted != message.Id - 1)
            {
                Console.WriteLine($"Wrong ID, Topic {topic}. Receive Id = {message.Id}, but lasted = {lasted}");
            }

            _lasted[topic] = message.Id;

            return new ValueTask();
        }

        private async ValueTask BidAskBatchCallback(IReadOnlyList<IMyServiceBusMessage> messages, string topic)
        {
            if (messages.Count == 0)
                return;

            var lasted = _lasted[topic];

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

            _lasted[topic] = max;

            //Console.WriteLine($"{topic}: {messages.Count}");

            return;
        }
    }
}