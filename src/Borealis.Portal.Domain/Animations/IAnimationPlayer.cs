using Borealis.Domain.Effects;
using Borealis.Domain.Ledstrips;



namespace Borealis.Portal.Domain.Animations;


/// <summary>
/// The animation player that can be used to play animations on ledstrips.
/// </summary>
public interface IAnimationPlayer : IAsyncDisposable
{
    /// <summary>
    /// The effect that we want to play
    /// </summary>
    Effect Effect { get; }

    /// <summary>
    /// The ledstrip that we are playing the animation for.
    /// </summary>
    Ledstrip Ledstrip { get; }

    /// <summary>
    /// A flag indicating that we are running.
    /// </summary>
    bool IsRunning { get; }


    /// <summary>
    /// Starts the player.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task StartAsync(CancellationToken token = default);


    /// <summary>
    /// Stops the player.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task PauseAsync(CancellationToken token = default);
}