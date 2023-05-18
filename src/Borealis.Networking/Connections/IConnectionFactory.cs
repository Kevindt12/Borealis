using Borealis.Networking.IO;



namespace Borealis.Networking.Connections;


/// <summary>
/// The factory that creates <see cref="IConnection" />
/// </summary>
public interface IConnectionFactory
{
	/// <summary>
	/// Creates the connection that we want to make.
	/// </summary>
	/// <param name="socket"> The <see cref="ISocket" /> that is providing that connection. </param>
	/// <returns> The <see cref="IConnection" /> that is managing that connection. </returns>
	IConnection CreateConnection(ISocket socket);
}