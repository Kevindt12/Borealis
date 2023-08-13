using System.Net;

using Borealis.Drivers.RaspberryPi.Sharp.Connection.Managers;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Options;
using Borealis.Networking.Connections;
using Borealis.Networking.IO;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Services;


public sealed class ConnectionHostedService : IHostedService, IDisposable
{
	private readonly ILogger<ConnectionHostedService> _logger;
	private readonly IConnectionFactory _connectionFactory;
	private readonly IConnectionManager _connectionManager;

	private readonly ISocketServer _socketServer;

	private CancellationTokenSource? _listeningCancellationTokenSource;


	public ConnectionHostedService(ILogger<ConnectionHostedService> logger,
								   ISocketServerFactory socketServerFactory,
								   IConnectionFactory connectionFactory,
								   IConnectionManager connectionManager,
								   IOptions<ServerOptions> serverOptions)
	{
		_logger = logger;
		_connectionFactory = connectionFactory;
		_connectionManager = connectionManager;

		_socketServer = socketServerFactory.CreateSocketServer(new IPEndPoint(IPAddress.Any, serverOptions.Value.Port));
	}


	/// <inheritdoc />
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Starting server to allow the portal to connect.");
		await StartListeningForClientsAsync(cancellationToken).ConfigureAwait(false);
	}


	/// <inheritdoc />
	public async Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Stopping the server.");

		_listeningCancellationTokenSource?.Cancel();
		if (_socketServer.IsRunning)
		{
			await _socketServer.StopAsync(cancellationToken).ConfigureAwait(false);
		}
	}


	private async Task StartListeningForClientsAsync(CancellationToken token = default)
	{
		_listeningCancellationTokenSource = new CancellationTokenSource();
		_ = Task.Factory.StartNew(async () =>
								  {
									  await ListenForClientsAsync(_listeningCancellationTokenSource.Token);
									  _listeningCancellationTokenSource = null;
								  },
								  CancellationToken.None,
								  TaskCreationOptions.LongRunning,
								  TaskScheduler.Current);
	}


	private async Task ListenForClientsAsync(CancellationToken token = default)
	{
		await _socketServer.StartAsync(token).ConfigureAwait(false);

		ISocket socket = await _socketServer.AcceptSocketAsync(token).ConfigureAwait(false);

		IConnection connection = _connectionFactory.CreateConnection(socket);
		await _connectionManager.SetCurrentConnectionAsync(connection, token);

		connection.ConnectionDisconnected += OnConnectionDisconnectedAsync;

		await _socketServer.StopAsync(token).ConfigureAwait(false);
	}


	private async Task OnConnectionDisconnectedAsync(Object? sender, ConnectionDisconnectedEventArgs e)
	{
		await StartListeningForClientsAsync().ConfigureAwait(false);
	}


	/// <inheritdoc />
	public void Dispose() { }
}