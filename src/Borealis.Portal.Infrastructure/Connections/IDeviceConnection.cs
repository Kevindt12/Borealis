using Borealis.Domain.Communication.Messages;
using Borealis.Portal.Domain.Devices;



namespace Borealis.Portal.Infrastructure.Connections;


public interface IDeviceConnection : IAsyncDisposable, IDisposable
{
    Device Device { get; }


    ValueTask SendFrameAsync(FrameMessage frameMessage);
}