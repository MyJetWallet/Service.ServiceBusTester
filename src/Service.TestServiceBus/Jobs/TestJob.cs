using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Telegram.Bot;

namespace Service.TestServiceBus.Jobs
{
    public class TestJob
    {
        public const string TopicName = "test-sb-topic";

        private readonly MyServiceBusTcpClient _client;
        private readonly ILogger<TestJob> _logger;
        private MyTaskTimer _timer;

        private TelegramBotClient _botApiClient;

        private DateTime _lasetReceiveTiem = DateTime.UtcNow;
        private DateTime _lastSendTime = DateTime.UtcNow.AddMinutes(1);

        public TestJob(MyServiceBusTcpClient client, ILogger<TestJob> logger)
        {
            _client = client;
            _logger = logger;
            _timer = new MyTaskTimer(nameof(TestJob), TimeSpan.FromSeconds(1), logger, DoTime);
        }

        public void Start()
        {
            if (!string.IsNullOrEmpty(Program.Settings.BotApiKey) && Program.Settings.TestChatId == 0)
            {
                Console.WriteLine("=== TestJob is disabled ===");
                return;
            }
            
            _botApiClient = new TelegramBotClient(Program.Settings.BotApiKey);
            _botApiClient.SendTextMessageAsync(Program.Settings.TestChatId, $"Service bus {Program.Settings.ServiceBusHostPort} start to test").GetAwaiter().GetResult();

            _client.CreateTopicIfNotExists(TopicName);
            _client.Subscribe(TopicName, "TestServiceBus", TopicQueueType.DeleteOnDisconnect, HandleMessage);
            _timer.Start();
        }

        private ValueTask HandleMessage(IMyServiceBusMessage msg)
        {
            var str = Encoding.UTF8.GetString(msg.Data.ToArray());

            _lasetReceiveTiem = DateTime.Parse(str);

            return ValueTask.CompletedTask;
        }

        private async Task DoTime()
        {
            var str = DateTime.UtcNow.ToString("O");

            var message = "";


            try
            {
                await _client.PublishAsync(TopicName, Encoding.UTF8.GetBytes(str), false);
                //Console.WriteLine($"TIMER: {DateTime.UtcNow}");
                

                if ((DateTime.UtcNow - _lasetReceiveTiem).TotalSeconds > 60)
                    message = $"do not receive messages in {(DateTime.UtcNow - _lasetReceiveTiem)}";
            }
            catch (Exception ex)
            {
                message = $"cannot send message to ServiceBUS\n{ex.ToString()}";
                Console.WriteLine($"ERROR: {message}");
            }

            if (!string.IsNullOrEmpty(message) && (DateTime.UtcNow - _lastSendTime).TotalMinutes >= 5)
            {
                await _botApiClient?.SendTextMessageAsync(Program.Settings.TestChatId, message);

                _lastSendTime = DateTime.UtcNow;
            }
        }
    }
}
