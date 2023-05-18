using System;
using System.Drawing;
using System.Linq;

using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Proxies;


public abstract class LedstripProxyBase : IDisposable
{
    /// <summary>
    /// The Id of this connection.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();


    // ReSharper disable once MemberCanBeProtected.Global
    /// <summary>
    /// The ledstrip settings that we base the proxy of.
    /// </summary>
    public Ledstrip Ledstrip { get; protected set; }


    /// <summary>
    /// A base class proxy that needs to handle the given abstract settings.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip settings. </param>
    protected LedstripProxyBase(Ledstrip ledstrip)
    {
        Ledstrip = ledstrip;
    }


    /// <summary>
    /// Sets the colors of a the ledstrip to the given array.
    /// </summary>
    /// <param name="colors"> The colors that we want to give the ledstrip. </param>
    public abstract void SetColors(ReadOnlyMemory<PixelColor> colors);


    /// <summary>
    /// Clears the ledstrip.
    /// </summary>
    public virtual void Clear()
    {
        SetColors(Enumerable.Repeat((PixelColor)Color.Black, Ledstrip.PixelCount).ToArray());
    }


    /// <summary>
    /// The disposal of this object.
    /// </summary>
    /// <param name="disposing"> </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing) { }
    }


    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}