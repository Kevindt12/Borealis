using System;
using System.Linq;

using Borealis.Drivers.Rpi.Animations;
using Borealis.Drivers.Rpi.Ledstrips;

using UnitsNet;



namespace Borealis.Drivers.Rpi.Contexts;


public class DisplayContext
{
    private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);


    private readonly ILogger<DisplayContext> _logger;
    private readonly AnimationPlayerFactory _animationPlayerFactory;


    private readonly Dictionary<LedstripProxyBase, AnimationPlayer?> _activeLedstrips;


    public DisplayContext(ILogger<DisplayContext> logger, AnimationPlayerFactory animationPlayerFactory)
    {
        _logger = logger;
        _animationPlayerFactory = animationPlayerFactory;

        _activeLedstrips = new Dictionary<LedstripProxyBase, AnimationPlayer?>();
    }


    /// <summary>
    /// Displays a single frame to the ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip that we want to show the frame on. </param>
    /// <param name="frame"> The frame that we want to show. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="InvalidOperationException"> Thrown when we already have a ledstrip active. </exception>
    public async Task DisplayFrameAsync(LedstripProxyBase ledstrip, ReadOnlyMemory<PixelColor> frame, CancellationToken token = default)
    {
        // Waiting until there are no operations happening.
        await _lock.WaitAsync(token).ConfigureAwait(false);

        _logger.LogInformation($"Displaying a single frame on ledstrip {ledstrip}.");

        // Checking if the ledstrip is not already active.
        if (_activeLedstrips.ContainsKey(ledstrip))
        {
            _lock.Release();

            throw new InvalidOperationException("Cannot display frame on ledstrip that is already busy.");
        }

        // Adding the ledstrip to the dictionary.
        _logger.LogTrace("Adding the ledstrip to the dictionary of active ledstrips.");
        _activeLedstrips.Add(ledstrip, null);

        // Setting the colors on the ledstrip.
        _logger.LogTrace("Display the frame on the ledstrip.");
        ledstrip.SetColors(frame);
    }


    /// <summary>
    /// Starts a new animation on a ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The <see cref="LedstripProxyBase" /> that we want to show the ledstrip on. </param>
    /// <param name="frequency"> The frequency that we want to play the animation at. </param>
    /// <param name="initialFrameBuffer">
    /// The initial frame buffer that we ant to give to the
    /// <see cref="AnimationPlayer" />.
    /// </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="InvalidOperationException"> When a ledstrip is already active. </exception>
    public async Task StartAnimationAsync(LedstripProxyBase ledstrip, Frequency frequency, ReadOnlyMemory<PixelColor>[] initialFrameBuffer, CancellationToken token = default)
    {
        await _lock.WaitAsync(token).ConfigureAwait(false);

        _logger.LogInformation($"Displaying a single frame on ledstrip {ledstrip}.");

        // Checking if the ledstrip is not already active.
        if (_activeLedstrips.ContainsKey(ledstrip) && _activeLedstrips[ledstrip] != null)
        {
            _lock.Release();

            throw new InvalidOperationException("Cannot display frame on ledstrip that is already busy.");
        }

        // Adding the ledstrip to the dictionary.
        _logger.LogTrace("Creating a new animation player for the ledstrip.");
        AnimationPlayer player = _animationPlayerFactory.CreateAnimationPlayer(ledstrip);

        _logger.LogTrace("Adding the player to the dictionary.");
        _activeLedstrips.Add(ledstrip, player);

        // Starting the player.
        await player.StartAsync(frequency, initialFrameBuffer, token);
    }


    public async Task ClearLedstripAsync(LedstripProxyBase ledstrip, CancellationToken token = default)
    {
        await _lock.WaitAsync(token).ConfigureAwait(false);

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

        _lock.Release();
    }


    public async Task ClearAllLedstripsAsync(CancellationToken token = default)
    {
        await DisposeAsync().ConfigureAwait(false);
    }


    private int exceptionCounter;


    /// <inheritdoc />
    public void Dispose() { }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (KeyValuePair<LedstripProxyBase, AnimationPlayer?> player in _activeLedstrips)
        {
            if (player.Value != null && player.Value.IsRunning)
            {
                await player.Value.StopAsync().ConfigureAwait(false);
            }
        }

        _activeLedstrips.Clear();
    }
}