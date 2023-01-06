using Borealis.Domain.Communication;
using Borealis.Domain.Communication.Messages;
using Borealis.Domain.Devices;
using Borealis.Portal.Domain.Connections;
using Borealis.Portal.Domain.Devices;
using Borealis.Portal.Domain.Exceptions;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Infrastructure.Connections;


internal abstract class DeviceConnectionBase : IDeviceConnection
{
    private readonly ILogger _logger;
    private readonly List<ILedstripConnection> _connections;


    /// <inheritdoc />
    public virtual Device Device { get; }

    /// <inheritdoc />
    public virtual IReadOnlyList<ILedstripConnection> LedstripConnections => _connections.AsReadOnly();


    protected DeviceConnectionBase(ILogger logger, Device device)
    {
        _logger = logger;
        Device = device;

        _connections = new List<ILedstripConnection>(CreateLedstripConnections(device));
    }


    protected virtual IEnumerable<LedstripConnection> CreateLedstripConnections(Device device)
    {
        if (device.Configuration == null) yield break;

        for (byte i = 0; i < Device.Configuration!.Ledstrips.Count; i++)
        {
            yield return new LedstripConnection(this, Device.Configuration!.Ledstrips[i], i);
        }
    }


    /// <inheritdoc />
    /// <exception cref="DeviceConnectionException"> When there is a problem with the connection to the device. </exception>
    public virtual async Task SendConfigurationAsync(LedstripSettings ledstripSettings, CancellationToken token = default)
    {
        _logger.LogDebug($"Sending new ledstrip configuration to device {Device.Id}.");

        // Creating the packet that we need to send.
        CommunicationPacket packet = CommunicationPacket.CreatePacketFromMessage(new ConfigurationMessage(ledstripSettings));

        // Waiting for the result packet.
        await SendPacketAsync(packet, token);
    }


    /// <summary>
    /// Sends a packet that we want to have confirmed.
    /// </summary>
    /// <param name="packet"> The packet that we want to receive. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns>
    /// A <see cref="CommunicationPacket" /> that we received back from the device. This can also be
    /// <see>
    /// <cref> null </cref>
    /// </see>
    /// if nothing was received.
    /// </returns>
    internal abstract Task SendPacketAsync(CommunicationPacket packet, CancellationToken token = default);


    #region IDisposable

    private bool _disposed;


    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed) return;

        Dispose(true);
        GC.SuppressFinalize(this);
    }


    protected virtual void Dispose(bool disposing)
    {
        if (disposing) { }
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!_disposed) return;

        await DisposeAsyncCore();

        Dispose(false);
        GC.SuppressFinalize(this);
    }


    protected virtual async ValueTask DisposeAsyncCore() { }

    #endregion
}