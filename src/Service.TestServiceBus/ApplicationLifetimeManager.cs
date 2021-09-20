using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyServiceBus.TcpClient;
using Service.TestServiceBus.Jobs;

namespace Service.TestServiceBus
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly MonitoringJob _job;
        private readonly MyServiceBusTcpClient _client;
        private readonly TestJob _testJob;

        public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime, ILogger<ApplicationLifetimeManager> logger,
            //MonitoringJob job,
            MyServiceBusTcpClient client,
            TestJob testJob
            )
            : base(appLifetime)
        {
            _logger = logger;
            //_job = job;
            _client = client;
            _testJob = testJob;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
            //_job.Start();
            _client.Start();
            _testJob.Start();
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
        }
    }
}
