using Borealis.Drivers.RaspberryPi.Sharp.Animations.Core;
using Borealis.Drivers.RaspberryPi.Sharp.Animations.Factories;
using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Proxies;

using UnitsNet;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Contexts;


public class DisplayState : IDisposable, IAsyncDisposable
{
	private readonly AnimationPlayerFactory _animationPlayerFactory;
	private readonly LedstripProxyBase _ledstripProxy;


	private IAnimationPlayer? _animationPlayer;
	private ReadOnlyMemory<PixelColor>? _displayingFrame;

	/// <summary>
	/// The ledstrip.
	/// </summary>
	public Ledstrip Ledstrip { get; }


	/// <summary>
	/// The ledstrip state object that holds and gets te state of the ledstrip, and its action.
	/// </summary>
	public DisplayState(AnimationPlayerFactory animationPlayerFactory, LedstripProxyBase ledstripProxy)
	{
		_animationPlayerFactory = animationPlayerFactory;
		_ledstripProxy = ledstripProxy;
		Ledstrip = ledstripProxy.Ledstrip;
	}


	/// <summary>
	/// Indicates if the ledstrip has an animation already attached to it
	/// </summary>
	/// <returns> A flag indicating that there is an animation attached. </returns>
	public virtual bool HasAnimation()
	{
		return _animationPlayer != null;
	}


	/// <summary>
	/// A flag indicating that the ledstrip is busy with something.
	/// </summary>
	/// <returns> </returns>
	public virtual bool IsBusy()
	{
		return HasAnimation() || _displayingFrame != null;
	}


	/// <summary>
	/// A flag indicating that we are just displaying an frame to the ledstrip.
	/// </summary>
	/// <returns> </returns>
	public virtual bool IsDisplayingFrame()
	{
		return _displayingFrame != null;
	}


	/// <summary>
	/// Checks if the animation player has an animation playing.
	/// </summary>
	/// <returns> </returns>
	public virtual bool IsAnimationPlaying()
	{
		return _animationPlayer != null && _animationPlayer.IsRunning;
	}


	/// <summary>
	/// Starts playing an animation on the ledstrip.
	/// </summary>
	/// <param name="frequency"> The <see cref="Frequency" /> of the animation. </param>
	/// <param name="initialFrameBuffer"> The Initial frame buffer of the animation. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <exception cref="InvalidLedstripStateException"> When the state is invalid for the animation to start. </exception>
	public virtual async Task StartAnimationAsync(Frequency frequency, ReadOnlyMemory<PixelColor>[] initialFrameBuffer, CancellationToken token = default)
	{
		if (IsBusy()) throw new InvalidLedstripStateException("The ledstrip is busy with displaying something.");

		try
		{
			// Creating and starting the animation player.
			IAnimationPlayer player = _animationPlayerFactory.CreateAnimationPlayer(_ledstripProxy);

			// Setting the animation player.
			_animationPlayer = player;

			// Starting the animation player
			await player.StartAsync(frequency, initialFrameBuffer, token).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			// Cleaning up the animation player.
			_animationPlayer?.Dispose();
			_animationPlayer = null;

			throw;
		}
	}


	/// <summary>
	/// Pauses the animation that we where playing.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <exception cref="InvalidLedstripStateException"> When there is no animation or no animation playing. </exception>
	public virtual async Task PauseAnimationAsync(CancellationToken token = default)
	{
		if (!HasAnimation()) throw new InvalidLedstripStateException("There is no animation connected to the ledstrip state.");
		if (!IsAnimationPlaying()) throw new InvalidLedstripStateException("The animation is not playing.");

		try
		{
			// Stopping the animation player.
			await _animationPlayer!.StopAsync(token).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			_animationPlayer?.Dispose();
			_animationPlayer = null;

			throw;
		}
	}


	/// <summary>
	/// Stops an animation by removing the core player.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <exception cref="InvalidLedstripStateException"> When there is no animation that can be stopped. </exception>
	public virtual async Task StopAnimationAsync(CancellationToken token = default)
	{
		if (!HasAnimation()) throw new InvalidLedstripStateException("There is no animation to stop.");

		try
		{
			// Stop the animation if we are playing the animation.
			if (IsAnimationPlaying())
			{
				await _animationPlayer!.StopAsync(token).ConfigureAwait(false);
			}
		}
		finally
		{
			_animationPlayer!.Dispose();
			_animationPlayer = null;
		}
	}


	/// <summary>
	/// Displays an frame on the ledstrip.
	/// </summary>
	/// <param name="frame"> The frame that we want to display. </param>
	/// <exception cref="InvalidLedstripStateException"> Thrown when the ledstrip is busy and a frame cannot be set. </exception>
	public virtual void DisplayFrame(ReadOnlyMemory<PixelColor> frame)
	{
		if (HasAnimation()) throw new InvalidLedstripStateException("The ledstrip is busy with an animation and cannot display the given frame.");

		try
		{
			_displayingFrame = frame;
			_ledstripProxy.SetColors(frame);
		}
		catch (Exception e)
		{
			_displayingFrame = null;
			_ledstripProxy.Clear();

			throw;
		}
	}


	/// <summary>
	/// Clears the current displaying frame.
	/// </summary>
	/// <exception cref="InvalidLedstripStateException"> Thrown when the ledstrip is busy with an animation. </exception>
	public virtual void ClearFrame()
	{
		if (HasAnimation()) throw new InvalidLedstripStateException("The ledstrip is busy with an animation and the frame cannot be cleared.");

		_displayingFrame = null;
		_ledstripProxy.Clear();
	}


	#region IDisposable

	private bool _disposed;


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
			_animationPlayer?.Dispose();

			// Clear the ledstrip.
			_ledstripProxy.Clear();
			_ledstripProxy.Dispose();
		}

		_animationPlayer = null;
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
		if (IsAnimationPlaying())
		{
			await _animationPlayer!.StopAsync().ConfigureAwait(false);
		}

		_animationPlayer?.Dispose();

		// Clear the ledstrip.
		_ledstripProxy.Clear();
		_ledstripProxy.Dispose();
	}

	#endregion
}