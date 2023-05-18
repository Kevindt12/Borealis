using System.Net.Sockets;

using Borealis.Portal.Infrastructure.Communication;
using Borealis.Portal.Infrastructure.Connectivity.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Overby.Extensions.AsyncBinaryReaderWriter;



namespace Borealis.Portal.Infrastructure.Connectivity.Handlers;


internal delegate Task<CommunicationPacket> ReceivedHandler(CommunicationPacket packet, CancellationToken token = default);



internal class CommunicationHandler : ICommunicationHandler
{
	private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

	private readonly ILogger<CommunicationHandler> _logger;
	private readonly IOptions<ConnectivityOptions> _connectivityOptions;
	private readonly IOptions<KeepAliveOptions> _keepAliveOptions;

	private readonly AsyncBinaryReader _reader;
	private readonly AsyncBinaryWriter _writer;


	private readonly CancellationTokenSource _stoppingToken;
	private readonly Thread _readingThread;
	private readonly ReceivedHandler _receivedHandler;
	private bool _writing;


	private readonly NetworkStream _stream;

	/// <summary>
	/// The client that we are connected to.
	/// </summary>
	public TcpClient TcpClient { get; }


	/// <summary>
	/// The handler responsible for the communication between the client and the server.
	/// </summary>
	/// <param name="logger"> </param>
	/// <param name="connectivityOptions"> The communication packet that we have with the device. </param>
	/// <param name="tcpClient"> The tcp client that we want to connect to. </param>
	public CommunicationHandler(ILogger<CommunicationHandler> logger,
								IOptions<ConnectivityOptions> connectivityOptions,
								IOptions<KeepAliveOptions> keepAliveOptions,
								TcpClient tcpClient,
								ReceivedHandler receivedHandler)
	{
		_logger = logger;
		_connectivityOptions = connectivityOptions;
		_keepAliveOptions = keepAliveOptions;
		_receivedHandler = receivedHandler;

		TcpClient = tcpClient;
		TcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
		TcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, keepAliveOptions.Value.Interval.Seconds);
		TcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, keepAliveOptions.Value.RetryCount);

		_stream = TcpClient.GetStream();

		_reader = new AsyncBinaryReader(_stream);
		_writer = new AsyncBinaryWriter(_stream);

		_stoppingToken = new CancellationTokenSource();
		_readingThread = new Thread(SafeRunningLoopHandler)
		{
			IsBackground = true,
			Name = $"Client Thread : {tcpClient.Client.RemoteEndPoint}"
		};

		_readingThread.Start();
	}


	#region Reading

	/// <summary>
	/// The safe wrapper over the running loop handler.
	/// </summary>
	private async void SafeRunningLoopHandler()
	{
		try
		{
			await RunningLoopHandler();
		}
		catch (Exception e)
		{
			_logger.LogError(e, "There was an error while reading from the device. There we are disconnecting from the device and destroying the connection.");

			// TODO: Destroy the connection.
		}
	}


	/// <summary>
	/// The running task loop.
	/// </summary>
	/// <returns> </returns>
	private async Task RunningLoopHandler()
	{
		_logger.LogTrace($"Start listening for packets from client : {TcpClient.Client.RemoteEndPoint}.");

		// Looping till we get data.
		while (!_stoppingToken!.Token.IsCancellationRequested)
		{
			if (_stream.DataAvailable && _writing == false)
			{
				await _lock.WaitAsync(_stoppingToken!.Token).ConfigureAwait(false);

				try
				{
					// Getting the packet that we received.
					CommunicationPacket packet = await ReceiveAsync(_stoppingToken.Token).ConfigureAwait(false);

					// Handling the incoming packet.
					CommunicationPacket replyPacket = await _receivedHandler(packet, _stoppingToken.Token).ConfigureAwait(false);

					// Sending the reply back to the server.
					await SendAsync(replyPacket, _stoppingToken.Token).ConfigureAwait(false);
				}
				catch (SocketException socketException)
				{
					_logger.LogError(socketException, "Connection problem while reading from device.");

					// Cleaning up and stopping the loop. via a new task because we could get problems since the DisposeAsync will call the awaiter on this method.
					// So we can best spawn a new thread to start handling a cleanup here.
					await Task.Run(DisposeAsync).ConfigureAwait(false);

					break;
				}
				catch (OperationCanceledException operationCanceledException)
				{
					_logger.LogWarning(operationCanceledException, "A operation was cancelled by the portal.");

					break;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, " A error occurred while trying to read package from device.");

					throw;
				}
				finally
				{
					_lock.Release();
				}
			}

			// Adding a 10 ms delay when checking.
			await Task.Delay(10);
		}

		_logger.LogTrace($"Stop listening to client : {TcpClient.Client.RemoteEndPoint}.");
	}

	#endregion


	#region Actions

	/// <summary>
	/// Sends a packet the the device with a response attached.
	/// </summary>
	/// <param name="packet"> The packet that we want to send to the device. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> The <see cref="CommunicationPacket" /> that we received from the device. </returns>
	public virtual async Task<CommunicationPacket> SendWithReplyAsync(CommunicationPacket packet, CancellationToken token = default)
	{
		try
		{
			// Lock
			await _lock.WaitAsync(token).ConfigureAwait(false);
			_writing = true;

			// Write the the device
			await SendAsync(packet, token).ConfigureAwait(false);

			// Reading the reply.
			CommunicationPacket replyPacket = await ReceiveAsync(token).ConfigureAwait(false);

			return replyPacket;
		}
		finally
		{
			_lock.Release();
			_writing = false;
		}
	}


	/// <summary>
	/// Sends a packet the the client.
	/// </summary>
	/// <param name="packet"> The packet that we want to send. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	protected virtual async Task SendAsync(CommunicationPacket packet, CancellationToken token = default)
	{
		try
		{
			ReadOnlyMemory<byte> packetBuffer = packet.CreateBuffer();

			await _writer.WriteAsync(Convert.ToUInt32(packetBuffer.Length), token).ConfigureAwait(false);
			await _writer.WriteAsync(packetBuffer.Span.ToArray(), token).ConfigureAwait(false);
			await _stream.FlushAsync(token).ConfigureAwait(false);
		}
		catch (IOException ioException)
		{
			if (ioException.InnerException is SocketException)
			{
				throw ioException.InnerException;
			}

			throw;
		}
	}


	protected virtual async Task<CommunicationPacket> ReceiveAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();

		// Creating a time out token.
		CancellationTokenSource timeoutToken = new CancellationTokenSource();
		CancellationTokenSource combinedToken = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutToken.Token);

		try
		{
			// Reading the packet length.
			uint packetLength = await _reader.ReadUInt32Async(token).ConfigureAwait(false);

			// Reading the packet with the given length.
			ReadOnlyMemory<byte> buffer = await _reader.ReadBytesAsync(Convert.ToInt32(packetLength), token).ConfigureAwait(false);

			// Converting it to a communication packet.
			CommunicationPacket receivedPacket = CommunicationPacket.FromBuffer(buffer);

			return receivedPacket;
		}
		catch (OperationCanceledException operationCanceledException)
		{
			if (timeoutToken.IsCancellationRequested)
			{
				throw new TimeoutException("The receive operation has timed out.", operationCanceledException);
			}

			throw;
		}
		catch (IOException ioException)
		{
			if (ioException.InnerException is SocketException)
			{
				throw ioException.InnerException;
			}

			throw;
		}
	}

	#endregion


	#region Disposable

	private readonly bool _disposed = false;


	/// <inheritdoc />
	public void Dispose()
	{
		if (_disposed) return;

		Dispose(true);
		GC.SuppressFinalize(this);
	}


	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_stream.Dispose();
		}

		_reader?.Dispose();
		_writer?.Dispose();
	}


	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (_disposed) return;

		await DisposeAsyncCore();

		Dispose(false);
		GC.SuppressFinalize(this);
	}


	protected virtual async ValueTask DisposeAsyncCore()
	{
		await _stream.DisposeAsync().ConfigureAwait(false);
	}

	#endregion
}