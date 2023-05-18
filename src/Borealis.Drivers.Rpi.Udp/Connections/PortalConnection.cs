using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Borealis.Drivers.Rpi.Commands;
using Borealis.Drivers.Rpi.Commands.Actions;
using Borealis.Drivers.Rpi.Exceptions;

using Overby.Extensions.AsyncBinaryReaderWriter;



// TODO: Add IOException. IOException is thrown when the connection has dropped. IOException and SocketException are both systemExceptions. Food for taught.


namespace Borealis.Drivers.Rpi.Connections;


public class PortalConnection : IDisposable, IAsyncDisposable
{
    private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    private readonly int ReceiveTimeout = 8000;

    // Dependencies.
    private readonly ILogger<PortalConnection> _logger;

    // Networking.
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;

    // The 
    private readonly IQueryHandler<ConnectCommand, ConnectedQuery> _connectHandler;
    private readonly ICommandHandler<SetFrameCommand> _setFrameHandler;
    private readonly ICommandHandler<StartAnimationCommand> _startAnimationHandler;
    private readonly ICommandHandler<StopAnimationCommand> _stopAnimationHandler;
    private readonly ICommandHandler<ConfigurationCommand> _configurationHandler;


    // Readers writers.
    private readonly AsyncBinaryReader _reader;
    private readonly AsyncBinaryWriter _writer;

    // Reading.
    private readonly CancellationTokenSource? _stoppingToken;
    private readonly Task? _readingTask;

    private bool _writing;

    // Keep alive.
    private DateTime _lastKeepAliveMessage;
    private readonly Timer _keepCheckAliveTimer;
    private const int KeepAliveCheckSeconds = 680;

    /// <summary>
    /// A event indicating that we are disconnecting from the portal.
    /// </summary>
    public event EventHandler? Disconnecting;


    /// <summary>
    /// The remote endpoint of the server we are now connected with.
    /// </summary>
    public virtual EndPoint RemoteEndPoint { get; init; }


    public PortalConnection(ILogger<PortalConnection> logger,
                            TcpClient client,
                            IQueryHandler<ConnectCommand, ConnectedQuery> connectHandler,
                            ICommandHandler<SetFrameCommand> setFrameHandler,
                            ICommandHandler<StartAnimationCommand> startAnimationHandler,
                            ICommandHandler<StopAnimationCommand> stopAnimationHandler,
                            ICommandHandler<ConfigurationCommand> configurationHandler
    )
    {
        _logger = logger;

        // Networking.
        _client = client;
        _connectHandler = connectHandler;
        _setFrameHandler = setFrameHandler;
        _startAnimationHandler = startAnimationHandler;
        _stopAnimationHandler = stopAnimationHandler;
        _configurationHandler = configurationHandler;
        _stream = client.GetStream();

        RemoteEndPoint = client.Client.RemoteEndPoint!;

        // Streaming
        _reader = new AsyncBinaryReader(_stream, Encoding.Default, true);
        _writer = new AsyncBinaryWriter(_stream, Encoding.Default, true);

        // Starting the listening task.
        _stoppingToken = new CancellationTokenSource();
        _readingTask = Task.Factory.StartNew(RunningLoopHandler, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }


    /// <summary>
    /// Checks the keep alive timer. This will be invoked ever so often and checked if the keep alive is still valid.
    /// </summary>
    /// <returns> </returns>
    private void CheckKeepAliveAsync()
    {
        // Checking the last keep alive message.
        if (_lastKeepAliveMessage.AddSeconds(KeepAliveCheckSeconds) < DateTime.Now)
        {
            // If we have passed the timer then we will 
            _logger.LogError("Portal connection has not send a keep alive message.");

            // Telling the application that we are not connected anymore.
            Disconnecting?.Invoke(this, EventArgs.Empty);

            // Cleaning up this object.
            Dispose();
        }
    }


    /// <summary>
    /// Sending request to get more frames for the frame buffer of a animation player.
    /// </summary>
    /// <param name="ledstripIndex"> The ledstrip index that we want to get the buffer for. </param>
    /// <param name="amount"> The amount of frames that we want to receive from the portal. </param>
    public virtual async Task<ReadOnlyMemory<PixelColor>[]> SendRequestForFrameBuffer(byte ledstripIndex, int amount)
    {
        await _lock.WaitAsync(_stoppingToken!.Token).ConfigureAwait(false);
        _writing = true;

        try
        {
            CommunicationPacket requestPacket = CommunicationPacket.CreatePacketFromMessage(new FrameBufferRequestMessage(amount, ledstripIndex));

            // Sending the request to the portal. Also handles the ack from the portal.
            _logger.LogTrace("Sending request for the frame buffer.");
            await SendAsyncCore(requestPacket).ConfigureAwait(false);

            // Reading the packet that we received.
            _logger.LogTrace("Waiting for a response packet from the portal.");
            CommunicationPacket responsePacket = await ReadAsyncCore(_stoppingToken!.Token).ConfigureAwait(false);

            if (responsePacket.Identifier == PacketIdentifier.FramesBuffer)
            {
                // Returning the received frame buffer.
                _logger.LogTrace("Response message received start decoding.");
                FramesBufferMessage message = responsePacket.ReadPayload<FramesBufferMessage>()!;

                // Telling the application that we are not writing anymore.
                _writing = false;

                await SendAcknowledgementPacketAsync().ConfigureAwait(false);

                // Returning the frames that we got.
                return message.Frames;
            }
            else if (requestPacket.Identifier == PacketIdentifier.Error)
            {
                // Handling error.
                _logger.LogTrace("Error received from te client. Start decoding the error message.");
                ErrorMessage message = responsePacket.ReadPayload<ErrorMessage>()!;

                // Telling the application that we are not writing anymore.
                _writing = false;

                // Throw the exception that something has happend on the portal.
                throw new PortalException($"There was a problem with picking up the frame buffer for ledstrip {ledstripIndex}.", message.Exception);
            }
            else
            {
                // Handling that we have a unknown packet.
                _logger.LogTrace($"Unknown packet received from the portal of length {responsePacket.Payload?.Length ?? 0}");
                ErrorMessage message = new ErrorMessage("Unknown packet received.");

                // Send to the device that we got a error with a unknown packet.
                await SendAsyncCore(CommunicationPacket.CreatePacketFromMessage(message)).ConfigureAwait(false);

                // Telling the application that we are not writing anymore.
                _writing = false;

                // Throwing an exception saying that we could not process the message.
                throw new PortalConnectionException("Unknown packet received from the portal.");
            }
        }
        catch (SocketException e)
        {
            _logger.LogError(e, "Connection error while trying to process the frame buffer.");

            // Telling the application that we are disconnecting.
            Disconnecting?.Invoke(this, EventArgs.Empty);

            // Cleaning the object up.
            await DisposeAsync();

            // Throwing a exception saying taht we had a connection error.
            throw new PortalConnectionException("The portal had a connection problem.", e);
        }

        finally
        {
            _lock.Release();
        }
    }


    #region Reading

    /// <summary>
    /// The running task loop.
    /// </summary>
    /// <returns> </returns>
    private async Task RunningLoopHandler()
    {
        _logger.LogTrace($"Start listening for packets from client : {_client.Client.RemoteEndPoint}.");

        // Looping till we get data.
        while (!_stoppingToken!.Token.IsCancellationRequested)
        {
            if (_stream.DataAvailable && _writing == false)
            {
                await _lock.WaitAsync(_stoppingToken!.Token).ConfigureAwait(false);

                try
                {
                    // Getting the packet that we received.
                    CommunicationPacket packet = await ReadAsyncCore(_stoppingToken.Token).ConfigureAwait(false);

                    // Handling the incoming packet.
                    await HandleIncomingPacket(packet, _stoppingToken.Token).ConfigureAwait(false);
                }
                catch (ApplicationException applicationException)
                {
                    _logger.LogWarning(applicationException, "Error while processing request.");
                }
                catch (SocketException socketException)
                {
                    _logger.LogError(socketException, "Socket exception.");

                    // Telling the application that we are disconnecting.
                    Disconnecting?.Invoke(this, EventArgs.Empty);

                    // Cleaning up and stopping the loop.
                    await DisposeAsync().ConfigureAwait(false);

                    // Stopping the loop.
                    break;
                }
                catch (IOException e)
                {
                    _logger.LogError(e, "Error with reading data from the tcp server.");

                    // Telling the application that we are disconnecting.
                    Disconnecting?.Invoke(this, EventArgs.Empty);

                    // Cleaning up and stopping the loop.
                    await DisposeAsync().ConfigureAwait(false);

                    break;
                }
                catch (OperationCanceledException operationCanceledException)
                {
                    _logger.LogWarning(operationCanceledException, "A opearion was cancelled by the portal.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "A error occurred while reading a packet from the portal.");

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

        _logger.LogTrace($"Stop listening to client : {_client.Client.RemoteEndPoint}.");
    }


    /// Handles a incoming packet.
    /// </summary>
    /// <param name="packet"> The <see cref="CommunicationPacket" /> that we received. </param>
    /// <param name="stoppingTokenToken"> </param>
    /// <param name="remoteEndPoint"> The <see cref="IPEndPoint" /> from the remote device. </param>
    protected virtual Task HandleIncomingPacket(CommunicationPacket packet, CancellationToken token) =>
        packet.Identifier switch
        {
            PacketIdentifier.Connect        => HandleConnectAsync(packet, token),
            PacketIdentifier.StartAnimation => HandleStartAnimationAsync(packet, token),
            PacketIdentifier.StopAnimation  => HandleStopAnimationAsync(packet, token),
            PacketIdentifier.Frame          => HandleFrameAsync(packet, token),
            PacketIdentifier.Configuration  => HandleConfiguration(packet, token),
            _                               => HandleUnknownPacketAsync(packet, token)
        };


    /// <summary>

    #endregion


    #region Receive Handlers

    protected virtual async Task HandleConnectAsync(CommunicationPacket packet, CancellationToken token)
    {
        ConnectedQuery result = default!;

        try
        {
            _logger.LogTrace("Handling connection request from portal.");
            ConnectMessage message = packet.ReadPayload<ConnectMessage>()!;

            result = await _connectHandler.Execute(new ConnectCommand
                                           {
                                               ConfigurationConcurrencyToken = message.ConfigurationConcurrencyToken,
                                               RemoteConnection = RemoteEndPoint
                                           })
                                          .ConfigureAwait(false);
        }
        catch (ApplicationException e)
        {
            _logger.LogError(e, "Sending to the portal that we have a problem the connection request.");
            await SendAsyncCore(CommunicationPacket.CreatePacketFromMessage(new ErrorMessage(e)), token).ConfigureAwait(false);

            throw;
        }
        catch (IOException e)
        {
            _logger.LogError(e, "Sending to portal that we have a problem processing the connection request.");
            await SendAsyncCore(CommunicationPacket.CreatePacketFromMessage(new ErrorMessage(e)), token).ConfigureAwait(false);

            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unknown error.");

            throw;
        }

        // Creating the reply packet.
        _logger.LogTrace("Creating reply packet for the portal.");
        ConnectedMessage replyMessage = new ConnectedMessage(result.IsConfigurationValid);
        CommunicationPacket replyPacket = CommunicationPacket.CreatePacketFromMessage(replyMessage);

        _logger.LogTrace("Sending the reply packet to the portal.");
        await SendAsyncCore(replyPacket, token).ConfigureAwait(false);

        _logger.LogTrace("Waiting for the acknowledgement from the protal.");
        CommunicationPacket acknowledgementPacket = await ReadAsyncCore(token).ConfigureAwait(false);

        if (acknowledgementPacket.IsAcknowledgement)
        {
            // Handle the acknowledgement of the packet.
            _logger.LogTrace("Acknowledgement received.");
        }
        else if (acknowledgementPacket.Identifier == PacketIdentifier.Error)
        {
            // Handle portal error.
            ErrorMessage errorMessage = acknowledgementPacket.ReadPayload<ErrorMessage>()!;

            throw new PortalException("There was a problem with creating the connection between the portal and the driver.", errorMessage.Exception);
        }
        else
        {
            // handle unknown or unexpected packet.
            throw new PortalConnectionException($"Unexpected packet received, of type {acknowledgementPacket.Identifier}.") { Data = { { "Packet", acknowledgementPacket } } };
        }
    }


    protected virtual async Task HandleStartAnimationAsync(CommunicationPacket packet, CancellationToken token)
    {
        StartAnimationMessage message = packet.ReadPayload<StartAnimationMessage>()!;

        try
        {
            // Execute the handler.
            _logger.LogTrace($"Request to start animation on ledstrip {message.LedstripIndex}, at frequency {message.Frequency.Hertz} Hz with initial frame buffer of {message.InitialFrameBuffer.Count()}");

            // Execute the command.
            await _startAnimationHandler.ExecuteAsync(new StartAnimationCommand
                                         {
                                             LedstripIndex = message.LedstripIndex,
                                             Frequency = message.Frequency,
                                             InitialFrameBuffer = message.InitialFrameBuffer.ToArray()
                                         })
                                        .ConfigureAwait(false);

            _logger.LogTrace("Animation handler has started.");

            // Telling the portal that we where successful.
            await SendAcknowledgementPacketAsync(token).ConfigureAwait(false);
        }
        catch (InvalidOperationException e)
        {
            // Handles a already active ledstrip error.
            _logger.LogError(e, "Error while starting animation. Sending the error back to the portal.");
            await SendAsyncCore(CommunicationPacket.CreatePacketFromMessage(new ErrorMessage(e)), token).ConfigureAwait(false);
        }
    }


    protected virtual async Task HandleStopAnimationAsync(CommunicationPacket packet, CancellationToken token)
    {
        StopAnimationMessage message = packet.ReadPayload<StopAnimationMessage>()!;

        // Execute the handler.
        _logger.LogTrace($"Request to stop the animation on ledstrip {message.LedstripIndex}.");

        await _stopAnimationHandler.ExecuteAsync(new StopAnimationCommand
                                    {
                                        LedstripIndex = message.LedstripIndex
                                    })
                                   .ConfigureAwait(false);

        // Telling the portal that we where successful.
        await SendAcknowledgementPacketAsync(token).ConfigureAwait(false);
    }


    protected virtual async Task HandleFrameAsync(CommunicationPacket packet, CancellationToken token)
    {
        // Reading the payload of the message.
        FrameMessage message = packet.ReadPayload<FrameMessage>()!;

        // Execute the handler.
        _logger.LogTrace($"Request to set single frame on {message.LedstripIndex}");

        await _setFrameHandler.ExecuteAsync(new SetFrameCommand
                               {
                                   LedstripIndex = message.LedstripIndex,
                                   Frame = message.Frame
                               })
                              .ConfigureAwait(false);

        // Telling the portal that we where successful.
        await SendAcknowledgementPacketAsync(token).ConfigureAwait(false);
    }


    protected virtual async Task HandleConfiguration(CommunicationPacket packet, CancellationToken token)
    {
        ConfigurationMessage message = packet.ReadPayload<ConfigurationMessage>()!;

        try
        {
            // Execute the handler.
            _logger.LogTrace($"Handling incoming configuration message {message.Settings.LogToJson()}");

            await _configurationHandler.ExecuteAsync(new ConfigurationCommand
                                        {
                                            DeviceConfiguration = message.Settings
                                        })
                                       .ConfigureAwait(false);

            // Telling the portal that we where successful.
            await SendAcknowledgementPacketAsync(token).ConfigureAwait(false);
        }
        catch (ApplicationException e)
        {
            // Telling the portal that we failed the operation.
            _logger.LogError(e, "Error while handling frame buffer. Sending the error back to the portal.");
            await SendAsyncCore(CommunicationPacket.CreatePacketFromMessage(new ErrorMessage(e)), token).ConfigureAwait(false);
        }
    }


    protected virtual async Task HandleUnknownPacketAsync(CommunicationPacket packet, CancellationToken token)
    {
        // _logger.LogError($"Unknown packet received from {remoteEndPoint}");
        await SendAsyncCore(CommunicationPacket.CreatePacketFromMessage(new ErrorMessage("Unknown communication packet.")), token);
    }

    #endregion


    #region Internal Functions

    /// <summary>
    /// Writes a message to the stream.
    /// </summary>
    /// <exception cref="SocketException"> If there is a problem with the connection. </exception>
    /// <param name="packet"> The packet that we want to send. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    protected virtual async Task SendAsyncCore(CommunicationPacket packet, CancellationToken token = default)
    {
        try
        {
            ReadOnlyMemory<byte> packetBuffer = packet.CreateBuffer();

            await _writer.WriteAsync(packetBuffer.Length, token).ConfigureAwait(false);
            await _writer.WriteAsync(packetBuffer.Span.ToArray(), token).ConfigureAwait(false);
            await _stream.FlushAsync(token).ConfigureAwait(false);
        }
        catch (SocketException e)
        {
            _logger.LogError(e, "There was a problem when writing a message to the portal.");

            throw;
        }
    }


    /// <summary>
    /// Reads a packet from the buffer.
    /// </summary>
    /// <exception cref="SocketException"> Thrown when there is a problem with the connection with the device. </exception>
    /// <exception cref="IOException"> Thrown when there is a problem with the streams that we are using to communicate with the device. </exception>
    /// <exception cref="OperationCanceledException"> Thrown when the operation was cancelled by the token. </exception>
    /// <exception cref="TimeoutException"> Thrown when the receive has not received anything for the given timeout time. </exception>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A <see cref="CommunicationPacket" /> that has been send by the portal. </returns>
    protected virtual async Task<CommunicationPacket> ReadAsyncCore(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        // Creating a time out token.
        CancellationTokenSource timeoutToken = new CancellationTokenSource();
        CancellationTokenSource combinedToken = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutToken.Token);

        try
        {
            timeoutToken.CancelAfter(ReceiveTimeout);

            // Reading the packet length.
            int packetLength = await _reader.ReadInt32Async(token).ConfigureAwait(false);

            // Reading the packet with the given length.
            ReadOnlyMemory<byte> buffer = await _reader.ReadBytesAsync(packetLength, token).ConfigureAwait(false);

            // Converting it to a communication packet.
            CommunicationPacket receivedPacket = CommunicationPacket.FromBuffer(buffer);

            return receivedPacket;
        }
        catch (OperationCanceledException operationCanceledException)
        {
            _logger.LogTrace(operationCanceledException, $"Operation was cancelled checking if it was cancelled by the Timeout Token {timeoutToken.IsCancellationRequested}");

            if (timeoutToken.IsCancellationRequested)
            {
                throw new TimeoutException("The receive operation has timed out.", operationCanceledException);
            }

            throw;
        }
        catch (IOException ioException)
        {
            _logger.LogTrace(ioException, "IOException Caught checking if its a wrapped Socket Exception.");

            if (ioException.InnerException is SocketException)
            {
                throw ioException.InnerException;
            }

            throw;
        }
    }


    /// <summary>
    /// Sends an acknowledgement packet to the portal.
    /// </summary>
    /// <exception cref="SocketException"> If there is a problem with the connection. </exception>
    /// <param name="token"> A token to cancel the current operation. </param>
    protected virtual async Task SendAcknowledgementPacketAsync(CancellationToken token = default)
    {
        // Writing the acknowledgement packet to the portal.
        _logger.LogTrace("Sending acknowledgement packet to portal.");
        await SendAsyncCore(CommunicationPacket.CreateAcknowledgementPacket(), token);
    }

    #endregion


    #region Disposable & AsyncDisposable

    private bool _disposed;


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
            // Disposing the keep alive timer.
            _keepCheckAliveTimer?.Dispose();

            // Stopping the reading task.
            _stoppingToken?.Cancel();
            _stoppingToken?.Dispose();

            if (_client.Connected)
            {
                try
                {
                    // Disconnect the client if we are still connected.
                    _client.Client.Disconnect(true);
                }
                catch (SocketException socketException)
                {
                    // Log the warning and ignore. We are disposing if we fail then we don't really care.
                    _logger.LogWarning(socketException, "There was a problem with disconnecting from the portal.");
                }
                catch (ObjectDisposedException objectDisposedException)
                {
                    // Log the warning and ignore. We are disposing of it anyway.
                    _logger.LogWarning(objectDisposedException, "Exception that the tcp client had already been disposed.");
                }
            }

            // Disposing of the client.
            _client.Dispose();
        }
    }


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
        // Stopping the keep alive timer.
        await _keepCheckAliveTimer.DisposeAsync();

        // Stopping the reading task.
        _stoppingToken?.Cancel();
        _stoppingToken?.Dispose();

        // Cleaning up the task.
        if (_readingTask != null)
        {
            try
            {
                await _readingTask;
            }
            catch (AggregateException aggregateException)
            {
                _logger.LogWarning(aggregateException, "Task had a error while cleaning up.");
            }
            catch (OperationCanceledException canceledException)
            {
                _logger.LogWarning(canceledException, "The task threw a operaion cancelled exception.");
            }
        }

        // Checking if the client is connected.
        if (_client.Connected)
        {
            try
            {
                _logger.LogTrace("Telling the client that we want to disconnect.");
                await _client.Client.DisconnectAsync(true);
            }
            catch (Exception e)
            {
                // Ignore don't really care if it does not work.
                _logger.LogWarning(e, "Exception caught while disposing of the portal connection, exception ignored since we are disposing of it.");
            }
            finally
            {
                _client.Dispose();
            }
        }
    }

    #endregion
}