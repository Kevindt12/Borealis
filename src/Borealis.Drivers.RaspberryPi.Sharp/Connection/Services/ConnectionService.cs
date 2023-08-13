using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Context;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Transmission;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;
using Borealis.Networking.Messages;

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
		DriverMessageTransmitter connection = _connectionContext.CurrentMessageTransmitter ?? throw new InvalidOperationException("The portal is not connected.");

		AnimationBufferReply animationBufferReply = await connection.AnimationBufferRequestAsync(new AnimationBufferRequest
																								     { LedstripId = ledstrip.Id.ToString(), RequestedFrameCount = amount },
																							     token)
																	.ConfigureAwait(false);

		_logger.LogDebug("Received frames from server.");

		return animationBufferReply.FrameBuffer.Select(DriverMessageTransmitter.ConvertFrameMessage).ToArray();
	}
}