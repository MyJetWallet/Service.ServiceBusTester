using System;

namespace Service.TestServiceBus.Domain.Models
{
    public interface IHelloMessage
    {
        string Message { get; set; }
    }
}
