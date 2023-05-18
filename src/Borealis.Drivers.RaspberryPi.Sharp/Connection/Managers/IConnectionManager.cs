using System.Net.Sockets;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Managers;


/// <summary>
/// The service that handles the connection and the action that can be taken on the connection.
/// </summary>
public interface IConnectionManager
{
    /// <summary>
    /// Sets the current portal connection.
    /// </summary>
    /// <param name="client"> The incoming <see cref="TcpClient" />. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task SetCurrentConnectionAsync(TcpClient client, CancellationToken token = default);


    /// <summary>
    /// Checks if we have a connection.
    /// </summary>
    /// <returns> A bool indicating that we have an connection. </returns>
    bool HasConnection();


    /// <summary>
    /// Disconnects from the portal.
    /// </summary>
    /// <returns> </returns>
    /// <exception cref="InvalidOperationException"> </exception>
    Task DisconnectAsync(CancellationToken token = default);
}