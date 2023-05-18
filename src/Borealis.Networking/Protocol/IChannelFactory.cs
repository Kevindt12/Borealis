using Borealis.Networking.Connections;



namespace Borealis.Networking.Protocol;


/// <summary>
/// The factory that creates <see cref="IChannel" />
/// </summary>
public interface IChannelFactory
{
	/// <summary>
	/// Creates the <see cref="IChannel" /> to be able to open an channel between to connected devices.
	/// </summary>
	/// <param name="connection">
	/// The <see cref="IConnection" /> that we are using for this
	/// <see cref="IChannel" />.
	/// </param>
	/// <returns> A <see cref="IChannel" /> that we are going to be using for the connection. </returns>
	IChannel CreateChannel(IConnection connection);
}