using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Contexts;


public class LedstripContext : IDisposable, IAsyncDisposable
{
    private readonly List<LedstripState> _ledstrips;


    public LedstripContext()
    {
        _ledstrips = new List<LedstripState>();
    }


    /// <summary>
    /// A flag indicating that we have animations running.
    /// </summary>
    /// <returns> A <see cref="bool" /> <c> true </c> when there are animations attached to a ledstrip. </returns>
    public virtual bool HasAnimations()
    {
        return _ledstrips.Any(x => x.HasAnimation());
    }


    /// <summary>
    /// Checks if we have any ledstrips.
    /// </summary>
    /// <returns>
    /// Returns <c> true </c> if the context is empty. Else it will return
    /// <c> false </c>.
    /// </returns>
    public virtual bool IsEmpty()
    {
        return !_ledstrips.Any();
    }


    /// <summary>
    /// Gets a single ledstrip by its given id.
    /// </summary>
    /// <param name="id"> The ledstrip id. </param>
    /// <returns> The <see cref="LedstripState" /> or null if there is none found. </returns>
    public virtual LedstripState? GetLedstripStateById(Guid id)
    {
        return _ledstrips.FirstOrDefault(l => l.Ledstrip.Id == id);
    }


    /// <summary>
    /// Gets all the ledstrip states that we know of.
    /// </summary>
    /// <returns> </returns>
    public virtual IEnumerable<LedstripState> GetLedstripStates()
    {
        return _ledstrips.AsReadOnly();
    }


    /// <summary>
    /// Loads a configuration to the context.
    /// </summary>
    /// <param name="ledstrips"> The <see cref="Ledstrip" /> that we want to </param>
    /// <exception cref="InvalidOperationException"> Thrown when there are still ledstrips being tracked by the context. </exception>
    public virtual void LoadLedstrips(IEnumerable<LedstripState> ledstrips)
    {
        if (_ledstrips.Any()) throw new InvalidOperationException("Cannot updated the ledstrip context when there are still ledstrips being tracked by the context.");

        foreach (LedstripState ledstripState in ledstrips)
        {
            _ledstrips.Add(ledstripState);
        }
    }


    /// <summary>
    /// Clears the ledstrip.
    /// </summary>
    /// <exception cref="InvalidOperationException"> Thrown when there are animation players attached to the ledstrip. </exception>
    public virtual void Clear()
    {
        if (_ledstrips.Any(x => x.HasAnimation())) throw new InvalidOperationException("Cannot set configuration while an animation is still connected to a ledstrip.");

        // Dispose of any ledstrips that we know of.
        _ledstrips.ForEach(x => x.Dispose());
        _ledstrips.Clear();
    }


    #region IDisposable

    private bool _disposed;


    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        Dispose(true);
        GC.SuppressFinalize(this);

        _disposed = true;
    }


    protected virtual void Dispose(bool disposing)
    {
        if (disposing) { }

        if (_ledstrips.Any(x => x.HasAnimation()))
        {
            foreach (LedstripState ledstripDriverState in _ledstrips.Where(x => x.HasAnimation()))
            {
                ledstripDriverState.ClearFrame();
            }
        }

        // Dispose of any ledstrips that we know of.
        _ledstrips.ForEach(x => x.Dispose());
        _ledstrips.Clear();
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
        if (_ledstrips.Any(x => x.HasAnimation()))
        {
            foreach (LedstripState ledstripState in _ledstrips.Where(x => x.HasAnimation()))
            {
                await ledstripState.DisposeAsync();
            }
        }
    }

    #endregion
}