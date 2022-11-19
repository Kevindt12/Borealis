using System.Drawing;

using Borealis.Portal.Rpi.Configurations;



namespace Borealis.Portal.Rpi.Ledstrips;


public abstract class LedstripProxyBase : IDisposable
{
    public LedstripSettings LedstripSettings { get; protected set; } = new LedstripSettings();

    public abstract void SetColors(ReadOnlySpan<Color> colors);

    public abstract void Clear();


    // TODO: Later correct.
    /// <inheritdoc />
    public virtual void Dispose() { }
}