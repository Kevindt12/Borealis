using Borealis.Domain.Communication;
using Borealis.Domain.Communication.Messages;
using Borealis.Domain.Effects;
using Borealis.Drivers.Rpi.Udp.Contexts;
using Borealis.Drivers.Rpi.Udp.Handlers;
using Borealis.Drivers.Rpi.Udp.Ledstrips;

using UnitsNet;



namespace Borealis.Drivers.Rpi.Udp.Services;


/***
 * TODO: Make it so we know what ledstrips are in the system.
 * When starting to play a animation check here to play it and stop it.
 * Handle the creation on the animation players, and the destruction. Also prevent from a animation player starting while there is a animation playing or changing anything on the ledstrip.
 */



public class VisualService : IDisposable, IAsyncDisposable
{
    private readonly ILogger<VisualService> _logger;
    private readonly ConnectionContext _connectionContext;
    private readonly LedstripContext _ledstripContext;


    private readonly Dictionary<LedstripProxyBase, AnimationPlayerHandler?> _activeLedstrips;


    public VisualService(ILogger<VisualService> logger, ConnectionContext connectionContext, LedstripContext ledstripContext)
    {
        _logger = logger;
        _connectionContext = connectionContext;
        _ledstripContext = ledstripContext;
        _activeLedstrips = new Dictionary<LedstripProxyBase, AnimationPlayerHandler?>();
    }


    public Task DisplayFrameAsync(LedstripProxyBase ledstrip, ReadOnlyMemory<PixelColor> frame, CancellationToken token = default)
    {
        _logger.LogInformation($"Displaying a single frame on ledstrip {ledstrip}.");

        // Checking if the ledstrip is not already active.
        if (_activeLedstrips.ContainsKey(ledstrip) && _activeLedstrips[ledstrip] != null)
        {
            throw new InvalidOperationException("Cannot display frame on ledstrip that is already busy.");
        }

        // Adding the ledstrip to the dictionary.
        _logger.LogTrace("Adding the ledstrip to the dictionary of active ledstrips.");
        _activeLedstrips.Add(ledstrip, null);

        _logger.LogTrace("Display the frame on the ledstrip.");
        ledstrip.SetColors(frame);

        return Task.CompletedTask;
    }


    public async Task StartAnimationAsync(LedstripProxyBase ledstrip, Frequency frequency, IEnumerable<ReadOnlyMemory<PixelColor>> initialFrameBuffer, CancellationToken token = default)
    {
        _logger.LogInformation($"Displaying a single frame on ledstrip {ledstrip}.");

        // Checking if the ledstrip is not already active.
        if (_activeLedstrips.ContainsKey(ledstrip) && _activeLedstrips[ledstrip] != null)
        {
            throw new InvalidOperationException("Cannot display frame on ledstrip that is already busy.");
        }

        // Adding the ledstrip to the dictionary.
        _logger.LogTrace("Creating a new animation player for the ledstrip.");

        AnimationPlayerHandler player = new AnimationPlayerHandler(ledstrip,
                                                                   async amount =>
                                                                   {
                                                                       _logger.LogDebug($"Sending request for {amount} of frames to portal.");
                                                                       await _connectionContext.Connection!.SendAsync(CommunicationPacket.CreatePacketFromMessage(new FrameBufferRequestMessage(amount, _ledstripContext.IndexOf(ledstrip))));
                                                                   });

        // Setting the initial frame buffer.
        _logger.LogDebug("Setting the initial frame buffer.");
        player.AddStackBuffer(initialFrameBuffer);

        _logger.LogTrace("Adding the player to the dictionary.");
        _activeLedstrips.Add(ledstrip, player);

        // Starting the player.
        await player.StartAsync(frequency, token);
    }


    public async Task ClearLedstripAsync(LedstripProxyBase ledstrip, CancellationToken token = default)
    {
        _logger.LogInformation("Clearing the ledstrip and animation of ledstrip.");

        if (_activeLedstrips.ContainsKey(ledstrip) && _activeLedstrips[ledstrip] != null)
        {
            _logger.LogDebug($"Stopping the animation on ledstrip {ledstrip}.");

            // Stops the animation on the ledstrip.
            await _activeLedstrips[ledstrip]!.StopAsync();
        }

        // Clear the ledstrip.
        _logger.LogDebug("Clearing the animation and the ledstrip.");
        _activeLedstrips.Remove(ledstrip);
        ledstrip.Clear();
    }


    private int exceptionCounter;


    public void ProcessIncomingFrameBuffer(LedstripProxyBase ledstrip, ReadOnlyMemory<PixelColor>[] frameBuffer, CancellationToken token = default)
    {
        _logger.LogTrace($"Incoming frame buffer of {frameBuffer.Length}.");

        try
        {
            // Adding the stack buffer to the ledstrip animation player.
            _activeLedstrips[ledstrip]!.AddStackBuffer(frameBuffer);
        }
        catch (Exception e)
        {
            // Ignore the exception that might happen just log it.
            if (exceptionCounter++ > 3)
            {
                _logger.LogError("There where multiple exception caught when processing a frame buffer received from the portal.");

                throw;
            }
        }
    }


    /// <inheritdoc />
    public void Dispose() { }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (KeyValuePair<LedstripProxyBase, AnimationPlayerHandler?> player in _activeLedstrips)
        {
            if (player.Value != null)
            {
                await player.Value.StopAsync();
            }
        }

        _activeLedstrips.Clear();
    }
}