using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Context;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Core;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;

using Microsoft.Extensions.Logging;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Services;


public class ConnectionService : IConnectionService
{
    private readonly ILogger<ConnectionService> _logger;
    private readonly ConnectionContext _connectionContext;


    public ConnectionService(ILogger<ConnectionService> logger, ConnectionContext connectionContext)
    {
        _logger = logger;
        _connectionContext = connectionContext;
    }


    /// <inheritdoc />
    public async Task<ReadOnlyMemory<PixelColor>[]> RequestFrameBufferAsync(Ledstrip ledstrip, Int32 amount, CancellationToken token = default)
    {
        _logger.LogDebug($"Requesting {amount} frame from server for ledstrip {ledstrip}");
        PortalConnection connection = _connectionContext.CurrentConnection ?? throw new InvalidOperationException("The portal is not connected.");

        IEnumerable<ReadOnlyMemory<PixelColor>> frames = await connection.RequestFrameBuffer(ledstrip.Id, amount, token).ConfigureAwait(false);
        _logger.LogDebug("Received frames from server.");

        return frames.ToArray();
    }
}