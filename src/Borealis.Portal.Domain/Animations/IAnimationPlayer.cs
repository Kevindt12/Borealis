using Borealis.Domain.Animations;
using Borealis.Portal.Domain.Connections;



namespace Borealis.Portal.Domain.Animations;


/// <summary>
/// The animation player that can be used to play animations on ledstrips.
/// </summary>
public interface IAnimationPlayer : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The animation that we want to play.
    /// </summary>
    Animation Animation { get; }

    /// <summary>
    /// The ledstrip we want to play it on.
    /// </summary>
    ILedstripConnection Ledstrip { get; }

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
    Task StopAsync(CancellationToken token = default);
}