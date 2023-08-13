using System;
using System.Linq;

using Borealis.Domain.Effects;
using Borealis.Domain.Ledstrips;
using Borealis.Portal.Core.Devices.Contexts;
using Borealis.Portal.Core.Exceptions;
using Borealis.Portal.Core.Ledstrips.Contexts;
using Borealis.Portal.Core.Ledstrips.State;
using Borealis.Portal.Domain.Animations;
using Borealis.Portal.Domain.Common;
using Borealis.Portal.Domain.Devices.Models;
using Borealis.Portal.Domain.Ledstrips.Services;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Ledstrips.Services;


internal class LedstripService : ILedstripService
{
	private readonly ILogger<LedstripService> _logger;

	private readonly DeviceConnectionContext _deviceConnectionContext;
	private readonly LedstripDisplayContext _ledstripDisplayContext;
	private readonly IAnimationPlayerFactory _animationPlayerFactory;


	public LedstripService(ILogger<LedstripService> logger,
						   DeviceConnectionContext deviceConnectionContext,
						   LedstripDisplayContext ledstripDisplayContext,
						   IAnimationPlayerFactory animationPlayerFactory)
	{
		_logger = logger;
		_deviceConnectionContext = deviceConnectionContext;
		_ledstripDisplayContext = ledstripDisplayContext;
		_animationPlayerFactory = animationPlayerFactory;
	}


	/// <inheritdoc />
	public LedstripStatus? GetLedstripStatus(Ledstrip ledstrip)
	{
		return _ledstripDisplayContext.GetLedstripDisplayState(ledstrip)?.Status;
	}


	/// <inheritdoc />
	public virtual async Task AttachEffectToLedstripAsync(Ledstrip ledstrip, Effect effect, CancellationToken token = default)
	{
		LedstripDisplayState state = _ledstripDisplayContext.GetLedstripDisplayState(ledstrip) ?? throw new InvalidOperationException("The device has no state attached to it.");

		_logger.LogDebug("Initializing effect for ledstrip.");

		if (state.Status != LedstripStatus.Idle || state.Status != LedstripStatus.PausedAnimation)
		{
			throw new InvalidOperationException("The ledstrip is already busy with a other task.");
		}

		// Cleaning up the old animation player if there is one
		if (state.AnimationPlayer?.Effect != effect)
		{
			if (state.AnimationPlayer != null)
			{
				// Cleanup the animation player.
				await state.AnimationPlayer.DisposeAsync();
			}

			// resetting the state.
			state.RemoveAnimationPlayer();

			// Setting the new animation player 
			_logger.LogDebug("Creating the animation player.");
			state.SetAnimationPlayer(await _animationPlayerFactory.CreateAnimationPlayerAsync(effect, state.Connection, token).ConfigureAwait(false));
		}
	}


	/// <inheritdoc />
	public virtual async Task StartAnimationAsync(Ledstrip ledstrip, CancellationToken token = default)
	{
		// Getting state and checking if its valid.
		LedstripDisplayState state = _ledstripDisplayContext.GetLedstripDisplayState(ledstrip) ?? throw new InvalidOperationException("The device has no state attached to it.");

		if (state.AnimationPlayer == null) throw new InvalidOperationException("There was not animation player loaded for this ledstrip.");

		_logger.LogDebug($"Starting animation {state.AnimationPlayer!.Effect.Id} on ledstrip {ledstrip.Name}");
		await state.AnimationPlayer.StartAsync(token).ConfigureAwait(false);
	}


	/// <inheritdoc />
	public virtual async Task StopLedstripAsync(Ledstrip ledstrip, CancellationToken token = default)
	{
		LedstripDisplayState state = _ledstripDisplayContext.GetLedstripDisplayState(ledstrip) ?? throw new InvalidOperationException("The device has no state attached to it.");
		_logger.LogDebug($"Stopping ledstrip {ledstrip.Name}");

		// Getting the player and stopping the animation player state if we have one.
		if (state.AnimationPlayer is IAnimationPlayer player)
		{
			await player.PauseAsync(token).ConfigureAwait(false);
		}
		else
		{
			await state.Connection.ClearAsync(token).ConfigureAwait(false);
			state.ColorCleared();
		}
	}


	/// <inheritdoc />
	public Task TestLedstripAsync(Ledstrip ledstrip, CancellationToken token = default)
	{
		throw new NotImplementedException();
	}


	/// <inheritdoc />
	/// <exception cref="InvalidOperationException"> Thrown when the ledstrip has already a active animation on it. </exception>
	/// <exception cref="OperationCanceledException"> When the operation has been cancelled. </exception>
	/// <exception cref="DeviceException"> Thrown when there is a that the device experienced. </exception>
	/// <exception cref="Domain.Exceptions.DeviceCommunicationException"> Thrown when the device has problems with the communication. </exception>
	/// <exception cref="Domain.Exceptions.DeviceConnectionException"> Thrown when there is a problem with the connection with the device. </exception>
	/// <exception cref="AnimationException"> Thrown when there is something that went wrong with the playing the animation. </exception>
	public async Task SetSolidColorAsync(Ledstrip ledstrip, PixelColor color, CancellationToken token = default)
	{
		LedstripDisplayState state = _ledstripDisplayContext.GetLedstripDisplayState(ledstrip) ?? throw new InvalidOperationException("The device has no state attached to it.");

		if (state.Status != LedstripStatus.Idle && state.Status != LedstripStatus.PausedAnimation) throw new InvalidOperationException("The current ledstrip is busy with a operation already.");

		// Removing the animation player if that was still selected.
		if (state.Status == LedstripStatus.PausedAnimation)
		{
			await state.AnimationPlayer!.DisposeAsync();
			state.RemoveAnimationPlayer();
		}

		ReadOnlyMemory<PixelColor> colors = Enumerable.Range(0, state.Ledstrip.Length).Select(x => color).ToArray();
		await state.Connection.DisplayFrameAsync(colors, token).ConfigureAwait(false);
		state.SetDisplayingColor(color);
	}


	/// <inheritdoc />
	public Effect? GetAttachedEffect(Ledstrip ledstrip)
	{
		return _ledstripDisplayContext.GetLedstripDisplayState(ledstrip)?.AnimationPlayer?.Effect;
	}


	/// <inheritdoc />
	public PixelColor? GetDisplayingColorOnLedstrip(Ledstrip ledstrip)
	{
		return _ledstripDisplayContext.GetLedstripDisplayState(ledstrip)?.ColorDisplaying;
	}
}