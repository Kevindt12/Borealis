using System;
using System.Net.Sockets;

using Borealis.Communication.Messages;
using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Communication;
using Borealis.Drivers.RaspberryPi.Sharp.Communication.Serialization;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Controllers;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Options;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;

using Google.FlatBuffers;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Overby.Extensions.AsyncBinaryReaderWriter;

using UnitsNet;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Core;


public class PortalConnection : IDisposable, IAsyncDisposable
{
    // Locks
    private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private bool _writing;

    private readonly PortalConnectionOptions _options;

    // Network connection.
    private readonly ILogger<PortalConnection> _logger;
    private readonly TcpClient _client;
    private readonly MessageSerializer _messageSerializer;
    private readonly ConnectionController _connectionController;
    private readonly NetworkStream _stream;

    // Reading Thread
    private readonly Thread _thread;
    private readonly CancellationTokenSource _threadCancellationTokenSource;


    // Binary buffers
    private readonly AsyncBinaryReader _reader;
    private readonly AsyncBinaryWriter _writer;

    /// <summary>
    /// A event indicating that we are disconnecting from the portal.
    /// </summary>
    public event EventHandler? Disconnecting;


    public PortalConnection(ILogger<PortalConnection> logger,
                            IOptions<PortalConnectionOptions> options,
                            TcpClient client,
                            MessageSerializer messageSerializer,
                            ConnectionController connectionController)
    {
        _options = options.Value;

        // Connection.
        _logger = logger;
        _client = client;
        _messageSerializer = messageSerializer;
        _connectionController = connectionController;
        _stream = client.GetStream();

        // Setting up the thread and its cancellation token.
        _threadCancellationTokenSource = new CancellationTokenSource();
        _thread = CreateReadingThread();
        _thread.Start();

        _reader = new AsyncBinaryReader(_stream);
        _writer = new AsyncBinaryWriter(_stream);
    }


    #region Writing

    /// <summary>
    /// Disconnects from the portal.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    public async Task DisconnectAsync(CancellationToken token = default)
    {
        _logger.LogTrace("Disconnecting from the portal.");
        await _client.Client.DisconnectAsync(true, token).ConfigureAwait(false);
    }


    /// <summary>
    /// Writes a message to the stream.
    /// </summary>
    /// <exception cref="SocketException"> If there is a problem with the connection. </exception>
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
        catch (SocketException e)
        {
            _logger.LogError(e, "There was a problem when writing a message to the portal.");

            throw;
        }
    }


    protected virtual async Task<CommunicationPacket> SendWithReplyAsync(CommunicationPacket packet, CancellationToken token = default)
    {
        // Lock
        await _lock.WaitAsync(token);
        _writing = true;

        // Sending
        await SendAsync(packet, token);

        // Receiving
        CommunicationPacket replyPacket = await ReadAsync(token);
        _lock.Release();
        _writing = false;

        return replyPacket;
    }


    /// <summary>
    /// Requests a animation buffer from the portal.
    /// </summary>
    /// <param name="ledstripId"> The id of the ledstrip of which we have an animation running and want to get the animation buffer from. </param>
    /// <param name="amount"> The amount of frames we want to get. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> The frames we get from the frame buffer. </returns>
    public virtual async Task<IEnumerable<ReadOnlyMemory<PixelColor>>> RequestFrameBuffer(Guid ledstripId, int amount, CancellationToken token = default)
    {
        // Build the message
        CommunicationPacket requestPacket = _messageSerializer.SerializeAnimationBufferRequest(ledstripId, amount);

        // Send and receive the reply packet.
        CommunicationPacket replyPacket = await SendWithReplyAsync(requestPacket, token).ConfigureAwait(false);

        // Reading reply packet.
        AnimationBufferReply reply = AnimationBufferReply.GetRootAsAnimationBufferReply(new ByteBuffer(replyPacket.Payload.ToArray()));

        return reply.UnPack().FrameBuffer.Select(x => new ReadOnlyMemory<PixelColor>(x.Pixels.Select(y => new PixelColor(y.R, y.G, y.B, y.W)).ToArray()));
    }

    #endregion


    #region Reading

    /// <summary>
    /// The running task loop.
    /// </summary>
    /// <returns> </returns>
    private async Task RunningLoopHandler()
    {
        _logger.LogTrace($"Start listening for packets from client : {_client.Client.RemoteEndPoint}.");
        CancellationToken stoppingToken = _threadCancellationTokenSource.Token;

        // Looping till we get data.
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_stream.DataAvailable && _writing == false)
            {
                await _lock.WaitAsync(stoppingToken).ConfigureAwait(false);

                try
                {
                    // Getting the packet that we received.
                    CommunicationPacket packet = await ReadAsync(stoppingToken).ConfigureAwait(false);

                    // Handling the incoming packet.
                    await HandleIncomingPacket(packet, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException operationCanceledException)
                {
                    _logger.LogWarning(operationCanceledException, "An operation was cancelled by the portal.");

                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "There was an error with the handling of the connection.");

                    // Telling the application that we are disconnecting.
                    Disconnecting?.Invoke(this, EventArgs.Empty);

                    // Cleaning up and stopping the loop.
                    await DisposeAsync().ConfigureAwait(false);

                    break;
                }
                finally
                {
                    _lock.Release();
                }
            }

            // Adding a 10 ms delay when checking.
            await Task.Delay(10, stoppingToken);
        }

        _logger.LogTrace($"Stop listening to client : {_client.Client.RemoteEndPoint}.");
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
    protected virtual async Task<CommunicationPacket> ReadAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        // Creating a time out token.
        CancellationTokenSource timeoutToken = new CancellationTokenSource();
        CancellationTokenSource combinedToken = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutToken.Token);

        try
        {
            timeoutToken.CancelAfter(_options.ReceiveTimeoutDuration);

            // Reading the packet length.
            UInt32 packetLength = await _reader.ReadUInt32Async(token).ConfigureAwait(false);

            // Reading the packet with the given length.
            byte[] buffer = await _reader.ReadBytesAsync(Convert.ToInt32(packetLength), token).ConfigureAwait(false);

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
    }


    protected virtual Task HandleIncomingPacket(CommunicationPacket packet, CancellationToken token) => packet.Identifier switch
    {
        PacketIdentifier.ConnectRequest          => HandleConnectRequestAsync(packet, token),
        PacketIdentifier.SetConfigurationRequest => HandleSetConfigurationRequest(packet, token),
        PacketIdentifier.StartAnimationRequest   => HandleStartAnimationRequest(packet, token),
        PacketIdentifier.PauseAnimationRequest   => HandlePauseAnimationRequestAsync(packet, token),
        PacketIdentifier.StopAnimationRequest    => HandleStopAnimationRequest(packet, token),
        PacketIdentifier.DisplayFrameRequest     => HandleSetLedstripFrameRequestAsync(packet, token),
        PacketIdentifier.ClearLedstripRequest    => HandleClearLedstripAsync(packet, token),
        PacketIdentifier.GetDriverStatusRequest  => HandleGetDriverStatusRequest(packet, token),
        PacketIdentifier.ErrorReply              => HandleErrorAsync(packet, token),
        _                                        => HandleUnknownPacketAsync(packet, token)
    };

    #endregion


    #region Handlers

    #region Connection And Configuration

    private async Task HandleConnectRequestAsync(CommunicationPacket packet, CancellationToken token = default)
    {
        _logger.LogTrace("Start handling new connection request from portal.");

        _messageSerializer.DeserializeConnectRequest(packet, out string configurationConcurrencyToken);

        ConnectResult connectResult = default!;

        try
        {
            // Setting the frame
            connectResult = await _connectionController.ConnectAsync(configurationConcurrencyToken, token).ConfigureAwait(false);
        }
        catch (InvalidConfigurationException invalidConfigurationException)
        {
            _logger.LogError(invalidConfigurationException, "There was an problem with the configuration of the device. ");
            await SendErrorReplyAsync(invalidConfigurationException, ErrorIds.UNKNOWNERROR, token).ConfigureAwait(false);

            throw;
        }
        catch (Exception systemException)
        {
            _logger.LogError(systemException, "There was an unexpected error while trying to connect to the portal. ");
            await SendErrorReplyAsync(systemException, ErrorIds.INTERNALERROR, token).ConfigureAwait(false);

            throw;
        }

        // Replying to the portal that we have set the frame.
        _logger.LogTrace("Sending packet that we successfully started the connection on the driver side.");
        CommunicationPacket replyPacket = _messageSerializer.SerializeConnectReply(connectResult);
        await SendAsync(replyPacket, token).ConfigureAwait(false);
    }


    protected virtual async Task HandleSetConfigurationRequest(CommunicationPacket packet, CancellationToken token = default)
    {
        _logger.LogTrace("Received new configuration for device.");

        // Reading the message.
        _messageSerializer.DeserializeSetConfigurationRequest(packet, out string configurationConcurrencyToken, out IEnumerable<Ledstrip> ledstrips);

        try
        {
            _logger.LogTrace("New configuration read start setting the new configuration.");
            await _connectionController.SetConfigurationAsync(ledstrips, configurationConcurrencyToken, token).ConfigureAwait(false);
        }
        catch (ValidationException validationException)
        {
            _logger.LogError(validationException, "Unable to set configuration because the validation failed.");
            CommunicationPacket errorReplyPacket = _messageSerializer.SerializeSetConfigurationReply($"Validation error, errors : {String.Join(", ", validationException.Errors)}");
            await SendAsync(errorReplyPacket, token).ConfigureAwait(false);
        }
        catch (InvalidOperationException invalidOperationException)
        {
            _logger.LogError(invalidOperationException, "There was an problem with setting the new configuration.");
            CommunicationPacket errorReplyPacket = _messageSerializer.SerializeSetConfigurationReply($"Error with setting the configuration, Error message : {invalidOperationException.Message}");
            await SendAsync(errorReplyPacket, token).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "There was an problem and could not set configuration.");
            await SendErrorReplyAsync(exception, ErrorIds.INTERNALERROR, token).ConfigureAwait(false);

            throw;
        }

        // Replying to the portal that we have set the frame.
        _logger.LogTrace("Sending reply that we have set the new configuration.");
        CommunicationPacket replyPacket = _messageSerializer.SerializeSetConfigurationReply(null);
        await SendAsync(replyPacket, token).ConfigureAwait(false);
    }


    protected virtual async Task HandleGetDriverStatusRequest(CommunicationPacket packet, CancellationToken token = default)
    {
        _logger.LogTrace("Received request of driver status.");

        GetDriverStatusRequest request = GetDriverStatusRequest.GetRootAsGetDriverStatusRequest(new ByteBuffer(packet.Payload.ToArray()));

        IEnumerable<DriverLedstripStatus> statuses = await _connectionController.GetDriverStatus(!String.IsNullOrEmpty(request.LedstripId) ? Guid.Parse(request.LedstripId) : null, token).ConfigureAwait(false);

        CommunicationPacket replyPacket = _messageSerializer.SerializeDriverStatusReply(statuses);

        await SendAsync(replyPacket, token).ConfigureAwait(false);
    }

    #endregion


    #region Animation

    protected virtual async Task HandleStartAnimationRequest(CommunicationPacket packet, CancellationToken token = default)
    {
        _logger.LogTrace("Received request to start animation on ledstrip.");
        _messageSerializer.DeserializeStartAnimationRequest(packet, out Guid ledstripId, out Frequency frequency, out IEnumerable<ReadOnlyMemory<PixelColor>> frames);

        try
        {
            // Starting the animation.
            await _connectionController.StartAnimationAsync(ledstripId, frequency, frames, token).ConfigureAwait(false);
        }
        catch (LedstripNotFoundException ledstripNotFoundException)
        {
            await HandleLedstripNotFoundExceptionAsync(ledstripNotFoundException, token).ConfigureAwait(false);
        }
        catch (InvalidLedstripStateException invalidLedstripStateException)
        {
            await HandleInvalidLedstripStateExceptionAsync(invalidLedstripStateException, token).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            await HandleUnexpectedExceptionAsync(exception, token).ConfigureAwait(false);

            throw;
        }

        // Replying to the portal that we have set the frame.
        _logger.LogTrace("Sending success reply to the portal.");
        await SendSuccessReplyAsync(token).ConfigureAwait(false);
    }


    protected virtual async Task HandlePauseAnimationRequestAsync(CommunicationPacket packet, CancellationToken token = default)
    {
        _logger.LogTrace("Received request to pause an animation.");
        _messageSerializer.DeserializePauseAnimationRequest(packet, out Guid ledstripId);

        try
        {
            // Stopping the ledstrip.
            await _connectionController.PauseAnimationAsync(ledstripId, token).ConfigureAwait(false);
        }
        catch (LedstripNotFoundException ledstripNotFoundException)
        {
            await HandleLedstripNotFoundExceptionAsync(ledstripNotFoundException, token).ConfigureAwait(false);
        }
        catch (InvalidLedstripStateException invalidLedstripStateException)
        {
            await HandleInvalidLedstripStateExceptionAsync(invalidLedstripStateException, token).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            await HandleUnexpectedExceptionAsync(exception, token).ConfigureAwait(false);

            throw;
        }

        // Replying to the portal that we have set the frame.
        _logger.LogTrace("Sending success reply to the portal.");
        await SendSuccessReplyAsync(token).ConfigureAwait(false);
    }


    protected virtual async Task HandleStopAnimationRequest(CommunicationPacket packet, CancellationToken token = default)
    {
        _logger.LogTrace("Received request to stop an animation.");

        _messageSerializer.DeserializeStopAnimationRequest(packet, out Guid ledstripId);

        try
        {
            // Stopping the ledstrip.
            await _connectionController.StopAnimationAsync(ledstripId, token).ConfigureAwait(false);
        }
        catch (LedstripNotFoundException ledstripNotFoundException)
        {
            await HandleLedstripNotFoundExceptionAsync(ledstripNotFoundException, token).ConfigureAwait(false);
        }
        catch (InvalidLedstripStateException invalidLedstripStateException)
        {
            await HandleInvalidLedstripStateExceptionAsync(invalidLedstripStateException, token).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            await HandleUnexpectedExceptionAsync(exception, token).ConfigureAwait(false);

            throw;
        }

        // Replying to the portal that we have set the frame.
        _logger.LogTrace("Sending success reply to the portal.");
        await SendSuccessReplyAsync(token).ConfigureAwait(false);
    }

    #endregion


    #region Static Colors

    protected virtual async Task HandleSetLedstripFrameRequestAsync(CommunicationPacket packet, CancellationToken token = default)
    {
        _logger.LogTrace("Received request to set ledstrip color.");

        _messageSerializer.DeserializeDisplayFrameRequest(packet, out Guid ledstripId, out ReadOnlyMemory<PixelColor> frame);

        try
        {
            // Setting the frame
            await _connectionController.SetLedstripFrameAsync(ledstripId, frame, token).ConfigureAwait(false);
        }
        catch (LedstripNotFoundException ledstripNotFoundException)
        {
            await HandleLedstripNotFoundExceptionAsync(ledstripNotFoundException, token).ConfigureAwait(false);
        }
        catch (InvalidLedstripStateException invalidLedstripStateException)
        {
            await HandleInvalidLedstripStateExceptionAsync(invalidLedstripStateException, token).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            await HandleUnexpectedExceptionAsync(exception, token).ConfigureAwait(false);

            throw;
        }

        // Replying to the portal that we have set the frame.
        _logger.LogTrace("Sending success reply to the portal.");
        await SendSuccessReplyAsync(token).ConfigureAwait(false);
    }


    protected virtual async Task HandleClearLedstripAsync(CommunicationPacket packet, CancellationToken token = default)
    {
        _logger.LogTrace("Received request to clear ledstrip color.");

        _messageSerializer.DeserializeClearLedstripRequest(packet, out Guid ledstripId);

        try
        {
            // Stopping the ledstrip.
            await _connectionController.ClearLedstripAsync(ledstripId, token).ConfigureAwait(false);
        }
        catch (LedstripNotFoundException ledstripNotFoundException)
        {
            await HandleLedstripNotFoundExceptionAsync(ledstripNotFoundException, token).ConfigureAwait(false);
        }
        catch (InvalidLedstripStateException invalidLedstripStateException)
        {
            await HandleInvalidLedstripStateExceptionAsync(invalidLedstripStateException, token).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            await HandleUnexpectedExceptionAsync(exception, token).ConfigureAwait(false);

            throw;
        }

        // Replying to the portal that we have set the frame.
        _logger.LogTrace("Sending success reply to the portal.");
        await SendSuccessReplyAsync(token).ConfigureAwait(false);
    }

    #endregion


    #region Replies

    /// <summary>
    /// Sends back that there was an error with the ledstrip state.
    /// </summary>
    /// <param name="exception"> The ledstrip state exception. </param>
    /// <param name="errorId"> The type of error that was thrown. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    private async Task SendErrorReplyAsync(Exception exception, ErrorIds errorId = ErrorIds.UNKNOWNERROR, CancellationToken token = default)
    {
        // Sending back an error when we encounter invalid ledstrip state.
        CommunicationPacket errorPacket = _messageSerializer.SerializeExceptionReply(errorId, exception);
        await SendAsync(errorPacket, token).ConfigureAwait(false);
    }


    /// <summary>
    /// Sends back that we where successful in the action that we wanted to perform.
    /// </summary>
    /// <param name="token"> </param>
    /// <returns> </returns>
    private async Task SendSuccessReplyAsync(CancellationToken token = default)
    {
        CommunicationPacket replyPacket = _messageSerializer.SerializeSuccessReply();
        await SendAsync(replyPacket, token).ConfigureAwait(false);
    }

    #endregion


    #region Error Handling

    /// <summary>
    /// Handles the ledstrip not found exception.
    /// </summary>
    /// <param name="ledstripNotFoundException">
    /// The
    /// <see cref="LedstripNotFoundException" /> exception that was thrown.
    /// </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    private async Task HandleLedstripNotFoundExceptionAsync(LedstripNotFoundException ledstripNotFoundException, CancellationToken token = default)
    {
        _logger.LogError(ledstripNotFoundException, "The ledstrip was not found.");
        await SendErrorReplyAsync(ledstripNotFoundException, ErrorIds.INTERNALERROR, token).ConfigureAwait(false);
    }


    /// <summary>
    /// Handles when an ledstrip was requested to do something that was not in an valid state.
    /// </summary>
    /// <param name="ledstripStateException">
    /// The
    /// <see cref="InvalidLedstripStateException" /> that was thrown.
    /// </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    private async Task HandleInvalidLedstripStateExceptionAsync(InvalidLedstripStateException ledstripStateException, CancellationToken token = default)
    {
        _logger.LogError(ledstripStateException, "The ledstrip was in an invalid state for the request.");
        await SendErrorReplyAsync(ledstripStateException, ErrorIds.INTERNALERROR, token).ConfigureAwait(false);
    }


    /// <summary>
    /// Handles and exception that was thrown when we where not expecting it.
    /// </summary>
    /// <param name="exception"> The <see cref="Exception" /> that was thrown </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    private async Task HandleUnexpectedExceptionAsync(Exception exception, CancellationToken token = default)
    {
        _logger.LogError(exception, "Unexpected exception was thrown.");
        await SendErrorReplyAsync(exception, ErrorIds.INTERNALERROR, token).ConfigureAwait(false);
    }


    /// <summary>
    /// Handles an unknown packet being received.
    /// </summary>
    /// <param name="packet"> The packet aht we received. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> </returns>
    protected virtual async Task HandleUnknownPacketAsync(CommunicationPacket packet, CancellationToken token = default)
    {
        _logger.LogError($"Unknown packet received from the device. PacketId: {packet.Identifier}, Packet size {packet.Payload.Length}.");

        // Sending back reply that we don't know what kind of packet we received.
        CommunicationPacket errorPacket = _messageSerializer.SerializeErrorReply(ErrorIds.INTERNALERROR, "Unknown packet received.");
        await SendAsync(errorPacket, token).ConfigureAwait(false);
    }


    protected virtual async Task HandleErrorAsync(CommunicationPacket packet, CancellationToken token = default)
    {
        // TODO: See for better implmentation of this.
        throw new Exception("There was an unexpected error received from the portal.");
    }

    #endregion

    #endregion


    #region Creation

    private void SafeReadThreadHandler()
    {
        Task result = Task.Factory.StartNew(RunningLoopHandler, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

        try
        {
            result.Wait();
        }
        catch (Exception e)
        {
            throw result.Exception!;
        }
    }


    private Thread CreateReadingThread()
    {
        Thread thread = new Thread(SafeReadThreadHandler);
        thread.IsBackground = true;
        thread.Name = "Portal Connection Reading Thread";
        thread.Priority = ThreadPriority.AboveNormal;

        return thread;
    }

    #endregion


    #region Disposing

    private bool _disposed;


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        await DisconnectAsync().ConfigureAwait(false);

        _client.Dispose();
    }


    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        _client.Dispose();
    }

    #endregion
}