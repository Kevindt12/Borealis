using Borealis.Drivers.RaspberryPi.Sharp.Connection.Core;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Context;


public class ConnectionContext : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// A event that is triggered when the connection has dropped or is disconnected.
    /// </summary>
    public event EventHandler? ConnectionDisconnected;


    /// <summary>
    /// The current connection.
    /// </summary>
    public PortalConnection? CurrentConnection { get; private set; }


    /// <summary>
    /// The connection context that holds the connection that we are connected to.
    /// </summary>
    public ConnectionContext() { }


    /// <summary>
    /// Sets the current connection.
    /// </summary>
    /// <param name="portalConnection"> The current connection we want to set. </param>
    public virtual void SetCurrentConnection(PortalConnection portalConnection)
    {
        if (CurrentConnection != null) throw new InvalidOperationException("The connection is still alive.");

        // Setting the current connection and listening to when the connection is dropped.
        CurrentConnection = portalConnection;
        CurrentConnection.Disconnecting += CurrentConnectionOnDisconnecting;
    }


    /// <summary>
    /// Clears the current connection.
    /// </summary>
    /// <param name="token"> CancellationToken token = default </param>
    /// <exception cref="InvalidOperationException"> Thrown when there is no connection to clear. </exception>
    public virtual async Task ClearCurrentConnectionAsync(CancellationToken token = default)
    {
        if (CurrentConnection == null) throw new InvalidOperationException("There is no current connection.");

        // Cleaning the connection.
        await CurrentConnection.DisposeAsync().ConfigureAwait(false);
        CurrentConnection = null;

        // Inform the application that we have disconnected from the portal.
        ConnectionDisconnected?.Invoke(this, EventArgs.Empty);
    }


    /// <summary>
    /// Triggered when the connection has dropped. We clean up the connection and dispose of it.
    /// </summary>
    private async void CurrentConnectionOnDisconnecting(Object? sender, EventArgs e)
    {
        await ClearCurrentConnectionAsync().ConfigureAwait(false);
    }


    /// <inheritdoc />
    public void Dispose()
    {
        CurrentConnection?.Dispose();
        CurrentConnection = null;
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (CurrentConnection != null)
        {
            await CurrentConnection.DisposeAsync().ConfigureAwait(false);
            CurrentConnection = null;
        }
    }
}