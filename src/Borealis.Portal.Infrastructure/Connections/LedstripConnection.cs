using Borealis.Domain.Communication;
using Borealis.Domain.Communication.Messages;
using Borealis.Domain.Devices;
using Borealis.Domain.Effects;
using Borealis.Portal.Domain.Connections;



namespace Borealis.Portal.Infrastructure.Connections;


internal class LedstripConnection : ILedstripConnection
{
    private readonly DeviceConnectionBase _deviceConnection;
    private readonly byte _ledstripIndex;


    /// <inheritdoc />
    public Ledstrip Ledstrip { get; }


    public LedstripConnection(DeviceConnectionBase parentConnection, Ledstrip ledstrip, byte ledstripIndex)
    {
        _deviceConnection = parentConnection;
        Ledstrip = ledstrip;
        _ledstripIndex = ledstripIndex;
    }


    /// <inheritdoc />
    public async ValueTask SendFrameAsync(ReadOnlyMemory<PixelColor> colors, CancellationToken token = default)
    {
        await _deviceConnection.SendUnconfirmedPacketAsync(CommunicationPacket.CreatePacketFromMessage(new FrameMessage(_ledstripIndex, Ledstrip.Colors, colors)));
    }


    public async Task<int> SendFramesBufferAsync(IEnumerable<ReadOnlyMemory<PixelColor>> frames, ColorSpectrum colors, CancellationToken token = default)
    {
        await _deviceConnection.SendUnconfirmedPacketAsync(CommunicationPacket.CreatePacketFromMessage(new FramesMessage(_ledstripIndex, Ledstrip.Colors, frames.Select(x => new FrameData(x, colors)))));
    }


    /// <inheritdoc />
    public async Task SetLedstripPixelsAsync(ReadOnlyMemory<PixelColor> colors, CancellationToken token = default)
    {
        await _deviceConnection.SendConfirmedPacketAsync(CommunicationPacket.CreatePacketFromMessage(new FrameMessage(_ledstripIndex, Ledstrip.Colors, colors)), token);
    }
}