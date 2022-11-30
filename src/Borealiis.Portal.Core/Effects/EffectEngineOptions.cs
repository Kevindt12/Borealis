namespace Borealis.Portal.Core.Effects;


/// <summary>
/// Options that we can pass tru to the effect engine.
/// </summary>
public class EffectEngineOptions
{
    /// <summary>
    /// The limit we want to give to the javascript engine.
    /// </summary>
    /// <remarks>
    /// We can work with large data sets so 128MB should be oke.
    /// </remarks>
    public long MemoryLimit { get; init; } = 2_000_000_000;


    /// <summary>
    /// The delegate that handles writing to the output log.
    /// </summary>
    public Action<string>? WriteLog { get; init; }

    /// <summary>
    /// The time out of a single javascript call. If its longer than this it will timeout.
    /// </summary>
    public TimeSpan TimeoutInterval { get; init; } = TimeSpan.FromSeconds(30);
}