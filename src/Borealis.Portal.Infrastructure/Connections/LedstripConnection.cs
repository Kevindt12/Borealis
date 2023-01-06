using Borealis.Domain.Communication;
using Borealis.Domain.Communication.Messages;
using Borealis.Domain.Devices;
using Borealis.Domain.Effects;
using Borealis.Portal.Domain.Connections;

using UnitsNet;



namespace Borealis.Portal.Infrastructure.Connections;


internal class LedstripConnection : ILedstripConnection
{
    private readonly DeviceConnectionBase _deviceConnection;
    private readonly byte _ledstripIndex;


    /// <inheritdoc />
    public event EventHandler<FramesRequestedEventArgs>? FramesRequested;

    /// <inheritdoc />
    public Ledstrip Ledstrip { get; }


    public LedstripConnection(DeviceConnectionBase parentConnection, Ledstrip ledstrip, byte ledstripIndex)
    {
        _deviceConnection = parentConnection;
        Ledstrip = ledstrip;
        _ledstripIndex = ledstripIndex;
    }


    /// <inheritdoc />
    public async Task SendFramesBufferAsync(IEnumerable<ReadOnlyMemory<PixelColor>> frames, CancellationToken token = default)
    {
        await _deviceConnection.SendPacketAsync(CommunicationPacket.CreatePacketFromMessage(new FramesBufferMessage(_ledstripIndex, Ledstrip.Colors, frames)), token);
    }


    /// <inheritdoc />
    public async Task StartAnimationAsync(Frequency frequency, IEnumerable<ReadOnlyMemory<PixelColor>> initialFrameBuffer, CancellationToken token = default)
    {
        await _deviceConnection.SendPacketAsync(CommunicationPacket.CreatePacketFromMessage(new StartAnimationMessage(frequency, _ledstripIndex, Ledstrip.Colors, initialFrameBuffer)), token);
    }


    /// <inheritdoc />
    public async Task StopAnimationAsync(CancellationToken token = default)
    {
        await _deviceConnection.SendPacketAsync(CommunicationPacket.CreatePacketFromMessage(new StopAnimationMessage(_ledstripIndex)), token);
    }


    /// <inheritdoc />
    public async Task SetSingleFrameAsync(ReadOnlyMemory<PixelColor> colors, CancellationToken token = default)
    {
        await _deviceConnection.SendPacketAsync(CommunicationPacket.CreatePacketFromMessage(new FrameMessage(_ledstripIndex, Ledstrip.Colors, colors)), token);
    }


    /// <inheritdoc />
    public void InvokeRequestForFFrames(Int32 amount)
    {
        FramesRequested?.Invoke(this, new FramesRequestedEventArgs(amount));
    }
}