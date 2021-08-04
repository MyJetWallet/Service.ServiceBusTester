using System.Runtime.Serialization;
using Service.TestServiceBus.Domain.Models;

namespace Service.TestServiceBus.Grpc.Models
{
    [DataContract]
    public class HelloMessage : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}