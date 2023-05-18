using System.Net.Sockets;

using Borealis.Drivers.RaspberryPi.Sharp.Connection.Context;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Controllers;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Core;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Factories;

using Microsoft.Extensions.Logging;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Managers;


public class ConnectionManager : IConnectionManager
{
    private readonly ILogger<ConnectionManager> _logger;
    private readonly ConnectionContext _connectionContext;
    private readonly PortalConnectionFactory _portalConnectionFactory;
    private readonly ConnectionControllerFactory _connectionControllerFactory;


    public ConnectionManager(ILogger<ConnectionManager> logger,
                             ConnectionContext connectionContext,
                             PortalConnectionFactory portalConnectionFactory,
                             ConnectionControllerFactory connectionControllerFactory
        )
    {
        _logger = logger;
        _connectionContext = connectionContext;
        _portalConnectionFactory = portalConnectionFactory;
        _connectionControllerFactory = connectionControllerFactory;
    }


    /// <summary>
    /// Sets the current portal connection.
    /// </summary>
    /// <param name="client"> The incoming <see cref="TcpClient" />. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    public virtual async Task SetCurrentConnectionAsync(TcpClient client, CancellationToken token = default)
    {
        _logger.LogDebug($"Setting current connection from {client.Client.RemoteEndPoint}.");

        // Creating the controller an portal.
        ConnectionController controller = _connectionControllerFactory.Create();
        PortalConnection connection = _portalConnectionFactory.Create(client, controller);

        _logger.LogTrace("Setting the connection context and tracking the connection.");
        _connectionContext.SetCurrentConnection(connection);
    }


    /// <summary>
    /// Checks if we have a connection.
    /// </summary>
    /// <returns> A bool indicating that we have an connection. </returns>
    public virtual bool HasConnection()
    {
        return _connectionContext.CurrentConnection != null;
    }


    /// <summary>
    /// Disconnects from the portal.
    /// </summary>
    /// <returns> </returns>
    /// <exception cref="InvalidOperationException"> </exception>
    public virtual async Task DisconnectAsync(CancellationToken token = default)
    {
        if (!HasConnection()) throw new InvalidOperationException("Cannot disconnect because there is no connection.");

        _logger.LogTrace("Disconnecting from the portal.");
        await _connectionContext.CurrentConnection!.DisconnectAsync(token).ConfigureAwait(false);
        await _connectionContext.ClearCurrentConnectionAsync(token).ConfigureAwait(false);

        _logger.LogTrace("Disconnected from the portal.");
    }
}