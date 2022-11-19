using System;
using System.Drawing;
using System.Linq;

using Borealis.Domain.Communication.Messages;
using Borealis.Domain.Devices;
using Borealis.Domain.Effects;
using Borealis.Portal.Domain.Devices;
using Borealis.Portal.Infrastructure.Connections;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Interaction;


internal abstract class LedstripInteractorBase : IDisposable, IAsyncDisposable
{
    private readonly ILogger _logger;

    private readonly IDeviceConnection _connection;
    private readonly byte _ledstripIndex;

    private bool _allowInteraction;

    /// <summary>
    /// The device we are connecting with.
    /// </summary>
    public virtual Device Device => _connection.Device;

    /// <summary>
    /// The ledstrip that we want to use to interact with.
    /// </summary>
    public virtual Ledstrip Ledstrip { get; }


    protected LedstripInteractorBase(ILogger logger, IDeviceConnection deviceConnection, Ledstrip ledstrip)
    {
        _logger = logger;
        _ledstripIndex = Convert.ToByte(deviceConnection.Device.Configuration?.Ledstrips.IndexOf(ledstrip) ?? throw new InvalidOperationException());
        _connection = deviceConnection;
        Ledstrip = ledstrip;
    }


    /// <summary>
    /// Starts the allowing interaction with the ledstrip.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> </returns>
    public async Task StartAsync(CancellationToken token = default)
    {
        if (_allowInteraction) throw new InvalidOperationException("Cannot start interactor that is already started.");

        _logger.LogTrace($"Starting Ledstrip interactor for {Ledstrip.Name} on Device {Device.Id}.");
        _allowInteraction = true;

        await OnStartAsync(token);
    }


    /// <summary>
    /// Called once the interactor is being started.
    /// </summary>
    /// <param name="token"> </param>
    /// <returns> </returns>
    protected abstract Task OnStartAsync(CancellationToken token);


    // TODO: Add Retry count and proper exception handling in the send colors.


    /// <summary>
    /// Sends the colors the ledstrip.
    /// </summary>
    /// <param name="colors"> </param>
    /// <returns> </returns>
    protected virtual async ValueTask SendColors(ReadOnlyMemory<PixelColor> colors)
    {
        if (!_allowInteraction) throw new InvalidOperationException("Cannot start interacting with a ledstrip if the interactor has not started.");

        await _connection.SendFrameAsync(new FrameMessage(_ledstripIndex, Ledstrip.Colors, colors));
    }


    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Cleaning up the context.
        }

        // Cleaning up resources.
    }


    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();

        Dispose(false);
        GC.SuppressFinalize(this);
    }


    protected virtual async ValueTask DisposeAsyncCore()
    {
        // Making sure we clear the ledstrip once we are done.
        await SendColors(Enumerable.Repeat((PixelColor)Color.Black, Ledstrip.Length).ToArray());
    }
}