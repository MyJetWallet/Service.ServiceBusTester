using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyServiceBus.TcpClient;
using Service.TestServiceBus.Jobs;

namespace Service.TestServiceBus.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var client = new MyServiceBusTcpClient(Program.ReloadedSettings(e => e.ServiceBusHostPort), "Tester");

            builder.RegisterInstance(client).AsSelf().SingleInstance();


            builder.RegisterType<MonitoringJob>().AsSelf().SingleInstance();


        }
    }
}