using Borealis.Domain.Communication.Messages;
using Borealis.Portal.Domain.Devices;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;
using Grpc.Net.Client;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Infrastructure.Connections;


public class GrpcDeviceConnection : IDeviceConnection
{
    private readonly GrpcChannel _channel;
    private readonly CoreService.CoreServiceClient _frameServiceClient;

    private AsyncClientStreamingCall<Frame, Empty>? _streamingCall;

    /// <inheritdoc />
    public Device Device { get; }


    /// <inheritdoc />
    public async ValueTask SendFrameAsync(FrameMessage frameMessage, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }


    /// <inheritdoc />
    public async Task SendConfirmedFrameAsync(FrameMessage frameMessage, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }


    /// <inheritdoc />
    public async Task SendConfigurationAsync(ConfigurationMessage configuration, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }


    protected GrpcDeviceConnection(ILogger<GrpcDeviceConnection> logger, Device device)
    {
        Device = device;

        UriBuilder builder = new UriBuilder();
        builder.Host = device.EndPoint.Address.ToString();
        builder.Port = device.EndPoint.Port;
        builder.Scheme = "http";

        _channel = GrpcChannel.ForAddress(builder.Uri,
                                          new GrpcChannelOptions());

        _frameServiceClient = new CoreService.CoreServiceClient(_channel);
    }


    /// <summary>
    /// Creates a new Grpc device connection and also connects to the device.
    /// </summary>
    /// <param name="logger"> The logger dependency </param>
    /// <param name="device"> The device we want to have a connection with. </param>
    /// <returns> A Grpc connection that we can use to talk to devices. </returns>
    public static async Task<GrpcDeviceConnection> CreateAsync(ILogger<GrpcDeviceConnection> logger, Device device)
    {
        return null;
    }


    protected virtual async Task StartStream() { }


    /// <inheritdoc />
    public async ValueTask SendFrameAsync(FrameMessage frameMessage)
    {
        if (_streamingCall is null) return;

        //await _streamingCall.RequestStream.WriteAsync(new Frame
        //{ LedstripIndex = frameMessage.LedstripIndex, Data = ByteString.CopyFrom(frameMessage.Colors)) });
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();

        Dispose(false);
        GC.SuppressFinalize(this);
    }


    public async ValueTask DisposeAsyncCore()
    {
        if (_streamingCall != null && _streamingCall.GetStatus().StatusCode == StatusCode.OK)
        {
            await _streamingCall.RequestStream.CompleteAsync();
            await _streamingCall;
        }

        _channel?.Dispose();
        _streamingCall?.Dispose();
    }


    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _channel?.Dispose();
            _streamingCall?.Dispose();
        }
    }
}