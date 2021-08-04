using System.ServiceModel;
using System.Threading.Tasks;
using Service.TestServiceBus.Grpc.Models;

namespace Service.TestServiceBus.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}