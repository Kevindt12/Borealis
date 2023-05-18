using Borealis.Networking.Communication;
using Borealis.Networking.Connections;

using Microsoft.Extensions.Logging;



namespace Borealis.Networking.Protocol;


internal class Channel : IChannel, IAsyncDisposable, IDisposable
{
	private const int PollingTime = 25;
	private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
	private bool _writing;

	private readonly ILogger<Channel> _logger;


	private Thread? _readingThread;
	private CancellationTokenSource? _readingCancellationTokenSource;


	/// <inheritdoc />
	public ChannelOptions ChannelOptions { get; }

	/// <inheritdoc />
	public virtual IConnection Connection { get; }

	/// <inheritdoc />
	public virtual ReceiveAsyncHandler? ReceiveAsyncHandler { get; set; }


	public Channel(ILogger<Channel> logger, IConnection connection, ChannelOptions? options = null)
	{
		_logger = logger;
		Connection = connection;

		ChannelOptions = options ?? new ChannelOptions();

		Connection.ConnectionDisconnected += OnConnectionDisconnectAsync;
	}


	private async Task OnConnectionDisconnectAsync(Object? sender, ConnectionDisconnectedEventArgs e)
	{
		if (IsReceivingThreadRunning())
		{
			StopReceivingThread();
		}
	}


	private Thread CreateThread()
	{
		Thread thread = new Thread(SafeReadingThreadHandlerAsync);
		thread.IsBackground = true;
		thread.Name = $"Connection {Connection.Socket.RemoteEndPoint} reading thread.";

		return thread;
	}


	/// <summary>
	/// Checks if the receiving thread is running.
	/// </summary>
	/// <returns> A <see cref="bool" /> indicating true if we ware running. </returns>
	/// <exception cref="InvalidOperationException"> When the thread and cancellation token are out of sync. </exception>
	private bool IsReceivingThreadRunning()
	{
		if ((_readingCancellationTokenSource == null) ^ (_readingThread == null)) throw new InvalidOperationException("The thread and cancellation token are both not in the same state. Total thread state invalid.");

		return (_readingCancellationTokenSource != null) & (_readingThread != null);
	}


	/// <summary>
	/// Starts the receiving thread.
	/// </summary>
	private void StartReceivingThread()
	{
		_logger.LogTrace("Starting the reading thread for channel {channel}", this);

		// Creating the thread.
		_readingCancellationTokenSource = new CancellationTokenSource();
		_readingThread = CreateThread();

		// Starting the thread.
		_readingThread.Start();
	}


	/// <summary>
	/// Stops the receiving thread.
	/// </summary>
	private void StopReceivingThread()
	{
		_logger.LogTrace("Stopping the receiving thread.");

		// Stopping the thread.
		_readingCancellationTokenSource!.Cancel();
		_readingThread!.Join(500);

		// Destroying the thread.
		_readingCancellationTokenSource.Dispose();
		_readingCancellationTokenSource = null;
		_readingThread = null;
	}


	private void SafeReadingThreadHandlerAsync()
	{
		try
		{
			ReadingThreadHandlerAsync().Wait();
		}
		catch (Exception e)
		{
			_logger.LogError(e, "An unexpected exception has occurred in the reading thread.");
		}
	}


	private async Task ReadingThreadHandlerAsync()
	{
		CancellationToken token = _readingCancellationTokenSource!.Token;
		while (!token.IsCancellationRequested)
		{
			if (ReceiveAsyncHandler != null && _writing == false && Connection.DataAvailable)
			{
				await _lock.WaitAsync(token).ConfigureAwait(false);
				try
				{
					// Reading the packet from the client.
					ReadOnlyMemory<byte> buffer = await Connection.ReceiveAsync(token).ConfigureAwait(false);
					CommunicationPacket packet = CommunicationPacket.FromBuffer(buffer);

					// Handling the packet.
					CommunicationPacket replyPacket = await ReceiveAsyncHandler.Invoke(packet, token);

					// Sending the reply to the server.
					await Connection.SendAsync(replyPacket.CreateBuffer(), token).ConfigureAwait(false);
				}
				finally
				{
					_lock.Release();
				}
			}

			await Task.Delay(PollingTime, token);
		}
	}


	/// <inheritdoc />
	public async Task OpenChannelAsync(CancellationToken token = default)
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		if (Connection.Socket.Connected) throw new InvalidOperationException("The channel is already connected.");

		_logger.LogTrace("Starting channel with the remote connection.");
		await Connection.ConnectAsync(token);

		StartReceivingThread();
	}


	/// <inheritdoc />
	public async Task CloseChannelAsync(CancellationToken token = default)
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		if (!Connection.Socket.Connected) throw new InvalidOperationException("The connection has not been started.");

		_logger.LogTrace("Starting channel with the remote connection.");
		await Connection.DisconnectAsync(token);

		StopReceivingThread();
	}


	/// <inheritdoc />
	public async ValueTask<CommunicationPacket> SendAsync(CommunicationPacket packet, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		CancellationToken linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, new CancellationTokenSource(ChannelOptions.ReceiveTimeout).Token).Token;

		// Locking the channel.
		await _lock.WaitAsync(token).ConfigureAwait(false);
		_writing = true;

		try
		{
			// Sending the packet to the client.
			await Connection.SendAsync(packet.CreateBuffer(), linkedToken).ConfigureAwait(false);

			// Reading the reply from the client.
			ReadOnlyMemory<byte> buffer = await Connection.ReceiveAsync(linkedToken).ConfigureAwait(false);
			CommunicationPacket replyPacket = CommunicationPacket.FromBuffer(buffer);

			return replyPacket;
		}
		catch (OperationCanceledException operationCanceledException)
		{
			_logger.LogWarning(operationCanceledException, "A timeout was reached by the cancellation token and no reply was received. Operation is cancelled.");

			throw new TimeoutException("The timeout has been reached by for this operation.", operationCanceledException);
		}
		finally
		{
			_writing = false;
			_lock.Release();
		}
	}


	#region IDisposable

	private bool _disposed;


	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (_disposed) return;

		await DisposeAsyncCore();

		Dispose(false);
		GC.SuppressFinalize(this);

		_disposed = true;
	}


	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (IsReceivingThreadRunning())
		{
			StopReceivingThread();
		}

		// Disposing of the connection.
		await Connection.DisposeAsync().ConfigureAwait(false);
	}


	/// <inheritdoc />
	public void Dispose()
	{
		if (_disposed) return;

		Dispose(true);
		GC.SuppressFinalize(this);

		_disposed = true;
	}


	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (IsReceivingThreadRunning())
			{
				StopReceivingThread();
			}

			// Disposing of the connection.
			Connection.Dispose();
		}
	}


	private void ThrowIfDisposed()
	{
		if (_disposed) throw new ObjectDisposedException(nameof(Channel), "The channel has already been disposed.");
	}

	#endregion
}