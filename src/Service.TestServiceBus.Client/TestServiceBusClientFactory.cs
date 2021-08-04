using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.TestServiceBus.Grpc;

namespace Service.TestServiceBus.Client
{
    [UsedImplicitly]
    public class TestServiceBusClientFactory: MyGrpcClientFactory
    {
        public TestServiceBusClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IHelloService GetHelloService() => CreateGrpcService<IHelloService>();
    }
}
