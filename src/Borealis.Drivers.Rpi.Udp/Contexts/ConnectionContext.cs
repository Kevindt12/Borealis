using Borealis.Drivers.Rpi.Udp.Connections;



namespace Borealis.Drivers.Rpi.Udp.Contexts;


public class ConnectionContext
{
    private readonly ILogger<ConnectionContext> _logger;


    private PortalConnection? _connection;

    public PortalConnection? Connection
    {
        get => _connection;
        set
        {
            if (_connection != null) throw new InvalidOperationException("Cannot set connection that is already active.");
            _connection = value;
        }
    }


    public ConnectionContext(ILogger<ConnectionContext> logger)
    {
        _logger = logger;
    }


    /// <summary>
    /// Disconnects the client from the portal.
    /// </summary>
    public async Task DisconnectPortalAsync()
    {
        await _connection!.DisposeAsync();

        _connection = null;
    }
}