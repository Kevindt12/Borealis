namespace Borealis.Portal.Core.Effects;


public class EffectEngineOptions
{
    /// <summary>
    /// The limit we want to give to the javascript engine.
    /// </summary>
    /// <remarks>
    /// We can work with large data sets so 128MB should be oke.
    /// </remarks>
    public long MemoryLimit { get; set; } = 2_000_000_000;


    /// <summary>
    /// The delegate that handles writing to the output log.
    /// </summary>
    public Action<string>? WriteLog { get; set; }
}