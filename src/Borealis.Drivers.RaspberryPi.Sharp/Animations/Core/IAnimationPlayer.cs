using Borealis.Drivers.RaspberryPi.Sharp.Common;

using UnitsNet;



namespace Borealis.Drivers.RaspberryPi.Sharp.Animations.Core;


public interface IAnimationPlayer : IDisposable
{
	/// <summary>
	/// The frequency we want to play the animation at.
	/// </summary>
	Frequency Frequency { get; set; }

	/// <summary>
	/// Indicates if the animation is running or not.
	/// </summary>
	bool IsRunning { get; }


	/// <summary>
	/// Starts a new animation of the ledstrip.
	/// </summary>
	/// <param name="frequency"> The frequency at which we want to play the animation at. </param>
	/// <param name="initialFrameBuffer">
	/// The <see cref="ReadOnlyMemory{T}" /> of
	/// <see cref="PixelColor" /> the frames that we want to start with.
	/// </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <exception cref="InvalidOperationException"> Thrown when the animation has already started. </exception>
	/// <exception cref="OperationCanceledException"> When the token has requested to stop the current operation. </exception>
	Task StartAsync(Frequency frequency, ReadOnlyMemory<PixelColor>[] initialFrameBuffer, CancellationToken token = default);


	/// <summary>
	/// Stops the animation that is playing on the ledstrip.
	/// </summary>
	/// <exception cref="InvalidOperationException"> The animation that we where playing. </exception>
	Task StopAsync(CancellationToken token = default);
}