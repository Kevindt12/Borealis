using System.Reflection;

using Borealis.Networking.Protocol;

using Microsoft.Extensions.Logging;



namespace Borealis.Networking.Transmission;


internal class MessageTransmitterAbstractFactory : IMessageTransmitterAbstractFactory
{
	private readonly ILoggerFactory _loggerFactory;
	private readonly IServiceProvider _serviceProvider;


	public MessageTransmitterAbstractFactory(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
	{
		_loggerFactory = loggerFactory;
		_serviceProvider = serviceProvider;
	}


	/// <inheritdoc />
	public TMessageTransmitter CreateMessageTransmitter<TMessageTransmitter>(IChannel channel) where TMessageTransmitter : MessageTransmitterBase
	{
		Type transmitterType = typeof(TMessageTransmitter);
		ConstructorInfo constructor = transmitterType.GetConstructors(BindingFlags.Public)[0];

		Dictionary<Type, object?> constructorValues = new Dictionary<Type, object?>(constructor.GetParameters().ToDictionary(x => x.ParameterType, x => x.DefaultValue));

		constructorValues[typeof(ILogger)] = _loggerFactory.CreateLogger<TMessageTransmitter>();
		constructorValues[typeof(IChannel)] = channel;

		foreach (Type parameterType in constructorValues.Keys)
		{
			if (constructorValues[parameterType] != null) continue;

			object value = _serviceProvider.GetService(parameterType)!;
			constructorValues[parameterType] = value;
		}

		return (TMessageTransmitter)Activator.CreateInstance(transmitterType, constructorValues.Values)!;
	}
}