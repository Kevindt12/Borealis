using Borealis.Networking.Protocol;



namespace Borealis.Networking.Transmission;


/// <summary>
/// The message transmitter factory used to create the implementations of the message transmitter via an abstraction over it.
/// </summary>
public interface IMessageTransmitterAbstractFactory
{
	/// <summary>
	/// Creates the message transmitter that we need ot use for communication.
	/// </summary>
	/// <typeparam name="TMessageTransmitter">
	/// The implemented type of the
	/// <see cref="MessageTransmitterBase" />.
	/// </typeparam>
	/// <param name="channel"> The <see cref="IChannel" /> that we want to use for the communication. </param>
	/// <returns> The <see cref="TMessageTransmitter" /> implementation that want to create from this. </returns>
	TMessageTransmitter CreateMessageTransmitter<TMessageTransmitter>(IChannel channel) where TMessageTransmitter : MessageTransmitterBase;
}