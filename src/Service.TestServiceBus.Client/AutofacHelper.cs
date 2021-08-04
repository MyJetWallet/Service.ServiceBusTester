using Autofac;
using Service.TestServiceBus.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.TestServiceBus.Client
{
    public static class AutofacHelper
    {
        public static void RegisterTestServiceBusClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new TestServiceBusClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetHelloService()).As<IHelloService>().SingleInstance();
        }
    }
}
