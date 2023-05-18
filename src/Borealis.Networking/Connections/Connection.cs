using System.Net.Sockets;

using Borealis.Networking.Exceptions;
using Borealis.Networking.IO;
using Borealis.Shared.Eventing;
using Borealis.Shared.Extensions;

using Microsoft.Extensions.Logging;



namespace Borealis.Networking.Connections;


internal class Connection : IConnection, IAsyncDisposable, IDisposable
{
	private const int ConnectionStatePollingTime = 40;

	private readonly ILogger<Connection> _logger;
	private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1);
	private readonly SemaphoreSlim _readLock = new SemaphoreSlim(1);

	private readonly AutoResetEvent _readingAutoResetEvent = new AutoResetEvent(false);

	private CancellationTokenSource? _connectionStateCancellationTokenSource;
	private Thread? _connectionStateThread;


	/// <summary>
	/// The handler used to keep track of te timers used for keep alives.
	/// </summary>
	private protected virtual KeepAliveHandler? KeepAliveHandler { get; set; }


	/// <inheritdoc />
	public event AsyncEventHandler<ConnectionDisconnectedEventArgs>? ConnectionDisconnected;

	/// <inheritdoc />
	public ISocket Socket { get; }


	/// <inheritdoc />
	public Boolean DataAvailable { get; protected set; }


	/// <inheritdoc />
	public ConnectionOptions ConnectionOptions { get; }


	public Connection(ILogger<Connection> logger, ISocket socket, ConnectionOptions? options = null)
	{
		_logger = logger;
		Socket = socket;
		ConnectionOptions = options ?? new ConnectionOptions();

		KeepAliveHandler = ConnectionOptions.KeepAlive.Enable ? new KeepAliveHandler(TimeSpan.FromSeconds(10), OnKeepAliveCheckHandlerReachedAsync) : null;
	}


	#region Keep Alive

	/// <summary>
	/// Handles the keep alive check handler.
	/// </summary>
	private async void OnKeepAliveCheckHandlerReachedAsync()
	{
		try
		{
			// Sending the buffer.
			await Socket.SendAsync(new ReadOnlyMemory<Byte>(FrameHeader.KeepAliveFrame().CreateBuffer().ToArray()), CancellationToken.None);
		}
		catch (SocketException e)
		{
			// Means the socket has closed.
			await DisposeAsyncCore().ConfigureAwait(false);
		}
	}

	#endregion


	#region Error Handling

	/// <summary>
	/// Handles a connection exception that has happened on an operation.
	/// </summary>
	/// <param name="socketException"> The socket exception that we have catched. </param>
	/// <remarks> The connection exception that we should throw. </remarks>
	private async ValueTask<ConnectionException> HandleSocketExceptionAsync(SocketException socketException)
	{
		if (socketException.SocketErrorCode is SocketError.ConnectionAborted or
										       SocketError.HostDown or
										       SocketError.NetworkDown or
										       SocketError.NetworkUnreachable or
										       SocketError.TimedOut)
		{
			// Stopping the connection state thread.
			if (IsConnectionStateThreadRunning())
			{
				StopConnectionStateThread();
			}

			// Create the exception.
			ConnectionException connectionException = new ConnectionException("THe connection has had a fatal error", socketException);

			// Inform the application.
			await OnConnectionDisconnectedAsync(new ConnectionDisconnectedEventArgs
												    { ConnectionException = connectionException });

			// If we want to dispose the connection when we have an disconnection.
			if (ConnectionOptions.DisposeOnDisconnection)
			{
				_logger.LogTrace("The socket exception was fatal disposing of connection.");
				await DisposeAsync();
			}

			return connectionException;
		}

		return new ConnectionException("There was an problem with the action that you where performing on the socket.", socketException);
	}

	#endregion


	#region Packing

	/// <summary>
	/// Generates the complete frame read to be send.
	/// </summary>
	/// <param name="contentBuffer"> The packet buffer we want to send. </param>
	/// <returns> The frame buffer we want to send. </returns>
	private ReadOnlyMemory<byte> GenerateFrameBuffer(ReadOnlyMemory<byte> contentBuffer)
	{
		// Create the header.
		FrameHeader header = new FrameHeader(FrameType.Packet, contentBuffer.Length);

		// Create the buffer.
		byte[] buffer = new Byte[FrameHeader.Length + header.ContentLength];

		// Copy data into the buffer.
		header.CreateBuffer().CopyTo(buffer);
		contentBuffer.CopyTo(buffer[FrameHeader.Length..]);

		return buffer;
	}

	#endregion


	#region Connection

	/// <inheritdoc />
	public async Task ConnectAsync(CancellationToken token = default)
	{
		ThrowIfDisposed();

		if (Socket.Connected) throw new ConnectionException("The client has already been connected.");

		try
		{
			// Starting the connection.
			_logger.LogTrace("Starting connection with {remoteEndpoint}.", Socket.RemoteEndPoint);
			await Socket.ConnectAsync(token).ConfigureAwait(false);
		}
		catch (SocketException socketException)
		{
			_logger.LogTrace(socketException, "Could not connect to remote host.");

			throw new ConnectionException("The connection was not able to be established with the remote connection.", socketException);
		}

		// Starting the thread.
		StartConnectionStateThread();
	}


	/// <inheritdoc />
	public async Task DisconnectAsync(CancellationToken token = default)
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		if (!Socket.Connected) throw new ConnectionException("The connection has not been connected.");

		// Stopping the connection state thread.
		StopConnectionStateThread();

		try
		{
			// Sends a disconnect message to the remote client.
			await Socket.SendAsync(FrameHeader.DisconnectFrame().CreateBuffer().ToArray(), CancellationToken.None).ConfigureAwait(false);
		}
		catch (SocketException e)
		{
			// TODO: Log the exception
		}

		// Stopping the connection with the socket.
		await Socket.DisconnectAsync(CancellationToken.None).ConfigureAwait(false);

		// Tells the application that we have disconnected.
		await OnConnectionDisconnectedAsync(new ConnectionDisconnectedEventArgs());
	}


	/// <summary>
	/// Sends out the event that we have disconnected.
	/// </summary>
	/// <param name="args"> </param>
	/// <returns> </returns>
	private async ValueTask OnConnectionDisconnectedAsync(ConnectionDisconnectedEventArgs args)
	{
		if (ConnectionDisconnected != null)
		{
			await ConnectionDisconnected.InvokeAsync(this, args);
		}
	}

	#endregion


	#region Action

	/// <inheritdoc />
	public async ValueTask SendAsync(ReadOnlyMemory<Byte> data, CancellationToken token = default)
	{
		ThrowIfDisposed();
		ThrowIfSocketDisconnected();
		token.ThrowIfCancellationRequested();

		// Lock the sending method.
		await _writeLock.WaitAsync(token).ConfigureAwait(false);

		try
		{
			// Generate the buffer.
			ReadOnlyMemory<byte> buffer = GenerateFrameBuffer(data);

			// Send to the client.
			await Socket.SendAsync(buffer, token).ConfigureAwait(false);
			KeepAliveHandler?.Alive();
		}
		catch (SocketException socketException)
		{
			throw await HandleSocketExceptionAsync(socketException);
		}
		finally
		{
			_writeLock.Release();
		}
	}


	/// <inheritdoc />
	public async ValueTask<ReadOnlyMemory<Byte>> ReceiveAsync(CancellationToken token = default)
	{
		ThrowIfDisposed();
		ThrowIfSocketDisconnected();
		token.ThrowIfCancellationRequested();

		await _readLock.WaitAsync(token).ConfigureAwait(false);

		try
		{
			// Reading the frame header.
			byte[] frameHeaderBuffer = new byte[FrameHeader.Length];
			await Socket.ReceiveAsync(frameHeaderBuffer, token).ConfigureAwait(false);
			FrameHeader header = FrameHeader.FromBuffer(frameHeaderBuffer);

			// Creating the frame content buffer.
			byte[] frameContentBuffer = new byte[header.ContentLength];

			// Reading the contents from the buffer.
			int readBytes = 0;
			while (readBytes < header.ContentLength)
			{
				readBytes += await Socket.ReceiveAsync(frameContentBuffer.AsMemory(readBytes), token).ConfigureAwait(false);
				KeepAliveHandler?.Alive();
			}

			return frameContentBuffer;
		}
		catch (SocketException socketException)
		{
			throw await HandleSocketExceptionAsync(socketException);
		}
		finally
		{
			DataAvailable = false;
			_readLock.Release();
			_readingAutoResetEvent.Set();
		}
	}

	#endregion


	#region Threading

	/// <summary>
	/// Creates the thread that we want to use to check keep hold of the connection state.
	/// </summary>
	/// <returns> The <see cref="Thread" /> that we will run to check the connection state. </returns>
	private Thread CreateThread()
	{
		Thread thread = new Thread(SafeThreadHandler);
		thread.IsBackground = true;

		return thread;
	}


	/// <summary>
	/// Starts the connection state thread.
	/// </summary>
	/// <exception cref="InvalidOperationException"> When the connection state thread has already started. </exception>
	private void StartConnectionStateThread()
	{
		// Guard.
		if (IsConnectionStateThreadRunning()) throw new InvalidOperationException("The connection state thread has already started.");

		// Creating the thread and cancellation token.
		_connectionStateCancellationTokenSource = new CancellationTokenSource();
		_connectionStateThread = CreateThread();

		// Starting the thread.
		_logger.LogTrace("Starting connection state thread for {connection}.", this);
		_connectionStateThread.Start();
	}


	/// <summary>
	/// Stops and cleans the connection state thread.
	/// </summary>
	/// <exception cref="InvalidOperationException"> When the thread has not yet started or is not running. </exception>
	private void StopConnectionStateThread()
	{
		// Guard 
		if (!IsConnectionStateThreadRunning()) throw new InvalidOperationException("The connection state thread has not started.");

		// Stopping the thread.
		_logger.LogTrace("Stopping connection state thread for {connection}.", this);
		_connectionStateCancellationTokenSource!.Cancel();
		_connectionStateThread!.Join(500);

		// Cleaning up the thread and cancellation token.
		_connectionStateCancellationTokenSource.Dispose();
		_connectionStateCancellationTokenSource = null;
		_connectionStateThread = null;
	}


	/// <summary>
	/// Checks if the thread is running.
	/// </summary>
	/// <returns>
	/// Returns <code>true</code> if the connection state thread is running else
	/// <code>false</code>.
	/// </returns>
	/// <exception cref="InvalidOperationException"> Thrown when the cancellation token and the thread are not in sync. Note this exception should not be handled and should crash the application. </exception>
	private bool IsConnectionStateThreadRunning()
	{
		if ((_connectionStateCancellationTokenSource == null) ^ (_connectionStateThread == null)) throw new InvalidOperationException("The thread and cancellation token are both not in the same state. Total thread state invalid.");

		return (_connectionStateCancellationTokenSource != null) & (_connectionStateThread != null);
	}

	#endregion


	#region Connection State Handling

	/// <summary>
	/// The thread handler made safe so we can log the exception that was thrown and dispose of the connection.
	/// </summary>
	private void SafeThreadHandler()
	{
		try
		{
			ConnectionStateThreadHandler().Wait();
		}
		catch (Exception e)
		{
			_logger.LogError(e, "There was an problem with the connection state thread handler.");

			Dispose();
		}
	}


	/// <summary>
	/// The core connection state thread handler.
	/// </summary>
	private async Task ConnectionStateThreadHandler()
	{
		// The core thread loop.
		CancellationToken token = _connectionStateCancellationTokenSource!.Token;
		while (!token.IsCancellationRequested)
		{
			// Checking if we have data on the buffer.
			if (Socket.DataAvailable > 0)
			{
				await CheckSocketBufferAsync(token).ConfigureAwait(false);
			}
			else
			{
				await Task.Delay(ConnectionStatePollingTime, token).ConfigureAwait(false);
			}
		}
	}


	/// <summary>
	/// Checks the socket buffer if we have an packet or not.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	private async Task CheckSocketBufferAsync(CancellationToken token = default)
	{
		await _readLock.WaitAsync(token).ConfigureAwait(false);

		bool isPacket = false;

		try
		{
			// Checking the frame header on the socket buffer if we have an packet and data that can be read.
			isPacket = await ReadSocketBufferFrameAsync(token).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			_readLock.Release();

			throw;
		}

		// Checking if its a packet.
		if (isPacket)
		{
			// We tell that we have data and wait until read function has been read.
			DataAvailable = true;
			_readLock.Release();
			_readingAutoResetEvent.WaitOne(TimeSpan.FromSeconds(10));
		}
		else
		{
			// If it is an status frame then we indicate that we don't have an packet and no data to be read.
			DataAvailable = false;
			_readLock.Release();
		}
	}


	/// <summary>
	/// Checks the socket buffer and checks if the data is in
	/// <see cref="FrameType.Packet" /> so we can read it or just a status frame.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> A <see cref="bool" /> indicating true if its a packet, else false. </returns>
	private async Task<bool> ReadSocketBufferFrameAsync(CancellationToken token = default)
	{
		// Peek the data from the socket.
		byte[] frameHeaderBuffer = new byte[FrameHeader.Length];
		await Socket.PeekAsync(frameHeaderBuffer, token).ConfigureAwait(false);
		FrameHeader header = FrameHeader.FromBuffer(frameHeaderBuffer);

		// If length is more then 0 then we have an packet that we received.
		if (header.FrameType == FrameType.Packet) return true;

		// Clear the frame header from the buffer.
		await Socket.ReceiveAsync(frameHeaderBuffer, token).ConfigureAwait(false);
		KeepAliveHandler?.Alive();

		// Check what we need to do.
		if (header.FrameType == FrameType.Disconnect)
		{
			await OnDisconnectionRequestAsync();
		}

		return false;
	}


	/// <summary>
	/// Handles the process of the client requesting the disconnection from us.
	/// </summary>
	/// <returns> </returns>
	private async Task OnDisconnectionRequestAsync()
	{
		// Disconnects from the socket.
		_logger.LogTrace("Remote client {remoteClient} requested to disconnect from us {connection}.", Socket.RemoteEndPoint, this);
		await Socket.DisconnectAsync();

		// Tell the application that we ware disconnecting.
		await OnConnectionDisconnectedAsync(new ConnectionDisconnectedEventArgs());

		// Dispose if we want the connection to dispose on disconnection.
		if (ConnectionOptions.DisposeOnDisconnection)
		{
			_ = Task.Run(DisposeAsync, CancellationToken.None);

			return;
		}

		// Else just stop the connection state thread.
		_ = Task.Run(StopConnectionStateThread);
	}

	#endregion


	#region IDisposalbe

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
		if (IsConnectionStateThreadRunning())
		{
			StopConnectionStateThread();
		}

		if (KeepAliveHandler != null)
		{
			await KeepAliveHandler.DisposeAsync();
		}

		Socket.Dispose();
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
			if (IsConnectionStateThreadRunning())
			{
				StopConnectionStateThread();
			}

			KeepAliveHandler?.Dispose();
			Socket.Dispose();
		}

		KeepAliveHandler = null;
	}


	private void ThrowIfDisposed()
	{
		if (_disposed) throw new ObjectDisposedException(nameof(Connection));
	}


	private void ThrowIfSocketDisconnected()
	{
		if (!Socket.Connected) throw new ConnectionException("The socket was not connected.");
	}

	#endregion
}