namespace Borealis.Networking.Connections;


internal sealed class KeepAliveHandler : IDisposable, IAsyncDisposable
{
	/// <summary>
	/// The handler that handles checking if an connection is still alive.
	/// </summary>
	public delegate void KeepAliveCheckHandler();



	private readonly TimeSpan _idleTime;
	private readonly Timer _timer;

	/// <summary>
	/// The keep alive check handler callback.
	/// </summary>
	public KeepAliveCheckHandler KeepAliveCheckHandlerCallback { get; }


	/// <summary>
	/// The class that handles the timers and checking of connection state.
	/// </summary>
	/// <param name="idleTime"> The time we should wait until we need to check that an connection is still alive. </param>
	/// <param name="keepAliveCheckHandler"> The callback when we check that a connection is still alive. </param>
	public KeepAliveHandler(TimeSpan idleTime, KeepAliveCheckHandler keepAliveCheckHandler)
	{
		_idleTime = idleTime;
		_timer = new Timer(OnCheck, null, idleTime, TimeSpan.MaxValue);

		KeepAliveCheckHandlerCallback = keepAliveCheckHandler;
	}


	/// <summary>
	/// Called when we need to check for the connection being alive.
	/// </summary>
	/// <param name="state"> Ignore this. </param>
	private void OnCheck(object? state)
	{
		KeepAliveCheckHandlerCallback.Invoke();
	}


	/// <summary>
	/// Indicates that we have performed an action on the connection so we can bump the keep alive time up.
	/// </summary>
	public void Alive()
	{
		_timer.Change(_idleTime, TimeSpan.MaxValue);
	}


	/// <inheritdoc />
	public void Dispose()
	{
		_timer.Dispose();
	}


	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		await _timer.DisposeAsync();
	}
}