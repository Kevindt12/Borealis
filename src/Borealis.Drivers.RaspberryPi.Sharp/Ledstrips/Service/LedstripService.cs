using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Contexts;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Factories;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Proxies;

using Microsoft.Extensions.Logging;

using UnitsNet;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Service;


public class LedstripService : ILedstripControlService, ILedstripConfigurationService
{
	private readonly ILogger<LedstripService> _logger;
	private readonly DisplayContext _displayContext;
	private readonly LedstripStateFactory _ledstripStateFactory;
	private readonly ILedstripProxyFactory _ledstripProxyFactory;


	public LedstripService(ILogger<LedstripService> logger,
						   DisplayContext displayContext,
						   LedstripStateFactory ledstripStateFactory,
						   ILedstripProxyFactory ledstripProxyFactory)
	{
		_logger = logger;
		_displayContext = displayContext;
		_ledstripStateFactory = ledstripStateFactory;
		_ledstripProxyFactory = ledstripProxyFactory;
	}


	#region Configuration

	/// <inheritdoc />
	public virtual async Task LoadConfigurationAsync(DeviceConfiguration configuration)
	{
		_logger.LogDebug("Loading configuration.");

		if (_displayContext.HasAnimations()) throw new InvalidOperationException("There are animation running on the ledstrip cannot ");

		// Cleaning the old configuration if there is one loaded.
		if (!_displayContext.IsEmpty())
		{
			_logger.LogTrace("Cleaning up the old ledstrip and loading in a new configuration.");
			_displayContext.Clear();
		}

		// Creating the ledstrip states.
		IEnumerable<DisplayState> states = CreateLedstripStates(configuration.Ledstrips);

		// Loading the ledstrip configuration.
		_logger.LogTrace("Loading ledstrip configuration.");
		_displayContext.LoadLedstrips(states);
	}


	/// <summary>
	/// Creates the ledstrip states that we need to load into the configuration.
	/// </summary>
	/// <param name="ledstrips"> The ledstrips that we want to load. </param>
	/// <returns> A <see cref="DisplayState" /> ready to be used. </returns>
	protected virtual IEnumerable<DisplayState> CreateLedstripStates(IEnumerable<Ledstrip> ledstrips)
	{
		foreach (Ledstrip ledstrip in ledstrips)
		{
			// Creating the ledstrip state.
			LedstripProxyBase ledstripProxy = _ledstripProxyFactory.CreateLedstripProxy(ledstrip);
			DisplayState state = _ledstripStateFactory.Create(ledstripProxy);

			yield return state;
		}
	}

	#endregion


	#region Information

	/// <inheritdoc />
	public virtual Ledstrip? GetLedstripById(Guid ledstripId)
	{
		return _displayContext.GetLedstripStateById(ledstripId)?.Ledstrip;
	}


	/// <inheritdoc />
	public virtual LedstripStatus? GetLedstripStatus(Ledstrip ledstrip)
	{
		return GetLedstripStatus(ledstrip.Id);
	}


	/// <inheritdoc />
	public virtual LedstripStatus? GetLedstripStatus(Guid ledstripId)
	{
		// Getting the ledstrip state
		DisplayState? ledstripState = _displayContext.GetLedstripStateById(ledstripId);

		if (ledstripState == null) return null;

		if (ledstripState.IsAnimationPlaying()) return LedstripStatus.PlayingAnimation;
		if (ledstripState.HasAnimation()) return LedstripStatus.PausedAnimation;
		if (ledstripState.IsDisplayingFrame()) return LedstripStatus.DisplayingFrame;

		return LedstripStatus.Idle;
	}

	#endregion


	#region Actions

	/// <inheritdoc />
	public virtual async Task StartAnimationAsync(Ledstrip ledstrip, Frequency frequency, ReadOnlyMemory<PixelColor>[] initialFrameBuffer, CancellationToken token = default)
	{
		// Getting the ledstrip state.
		_logger.LogDebug($"Starting animation on ledstrip {ledstrip.Id}, with frequency of {frequency.Hertz}Hz, with an initial frame buffer of {initialFrameBuffer.Length} frames.");
		DisplayState displayState = _displayContext.GetLedstripStateById(ledstrip.Id) ?? throw new LedstripNotFoundException("The selected ledstrip was not found.");

		await displayState.StartAnimationAsync(frequency, initialFrameBuffer, token).ConfigureAwait(false);
		_logger.LogDebug($"Animation started on ledstrip {ledstrip.Id}.");
	}


	/// <inheritdoc />
	public virtual async Task PauseAnimationAsync(Ledstrip ledstrip, CancellationToken token = default)
	{
		// Getting the ledstrip state.
		_logger.LogDebug($"Pausing animation on ledstrip {ledstrip.Id}");
		DisplayState displayState = _displayContext.GetLedstripStateById(ledstrip.Id) ?? throw new LedstripNotFoundException("The selected ledstrip was not found.");

		// Pausing the animation player.
		await displayState.PauseAnimationAsync(token).ConfigureAwait(false);
		_logger.LogDebug("The animation player has paused.");
	}


	/// <inheritdoc />
	public virtual async Task StopAnimationAsync(Ledstrip ledstrip, CancellationToken token = default)
	{
		// Getting the ledstrip state.
		_logger.LogDebug($"Stopping animation on ledstrip {ledstrip.Id}.");
		DisplayState displayState = _displayContext.GetLedstripStateById(ledstrip.Id) ?? throw new LedstripNotFoundException("The selected ledstrip was not found.");

		await displayState.StopAnimationAsync(token).ConfigureAwait(false);
		_logger.LogDebug("Animation player cleaned up.");
	}


	/// <inheritdoc />
	public async Task StopAnimations(CancellationToken token = default)
	{
		foreach (DisplayState ledstripState in _displayContext.GetLedstripStates().Where(x => x.HasAnimation()))
		{
			if (ledstripState.HasAnimation())
			{
				await ledstripState.StopAnimationAsync(token).ConfigureAwait(false);
			}
		}
	}


	/// <inheritdoc />
	public virtual Task DisplayFameAsync(Ledstrip ledstrip, ReadOnlyMemory<PixelColor> frame, CancellationToken token = default)
	{
		// Getting ledstrip state.
		_logger.LogDebug($"Displaying frame on ledstrip {ledstrip.Id}.");
		DisplayState displayState = _displayContext.GetLedstripStateById(ledstrip.Id) ?? throw new LedstripNotFoundException("The selected ledstrip was not found.");

		// Setting the frame.
		displayState.DisplayFrame(frame);
		_logger.LogDebug("Showing the colors on the ledstrip.");

		return Task.CompletedTask;
	}


	/// <inheritdoc />
	public virtual Task ClearLedstripAsync(Ledstrip ledstrip, CancellationToken token = default)
	{
		// Getting ledstrip state.
		_logger.LogDebug($"Displaying frame on ledstrip {ledstrip.Id}.");
		DisplayState displayState = _displayContext.GetLedstripStateById(ledstrip.Id) ?? throw new LedstripNotFoundException("The selected ledstrip was not found.");

		// Setting the frame.
		displayState.ClearFrame();
		_logger.LogDebug("Clearing the ledstrip.");

		return Task.CompletedTask;
	}

	#endregion
}