using System;
using System.Linq;

using Borealis.Drivers.Rpi.Commands.Actions;
using Borealis.Drivers.Rpi.Contexts;
using Borealis.Drivers.Rpi.Ledstrips;



namespace Borealis.Drivers.Rpi.Commands.Handlers;


public class SetFrameCommandHandler : ICommandHandler<SetFrameCommand>
{
    private readonly ILogger<SetFrameCommandHandler> _logger;
    private readonly LedstripContext _ledstripContext;
    private readonly DisplayContext _displayContext;


    public SetFrameCommandHandler(ILogger<SetFrameCommandHandler> logger, LedstripContext ledstripContext, DisplayContext displayContext)
    {
        _logger = logger;
        _ledstripContext = ledstripContext;
        _displayContext = displayContext;
    }


    /// <inheritdoc />
    public async Task ExecuteAsync(SetFrameCommand command)
    {
        // Getting the ledstrip that we want to set a frame on.
        _logger.LogDebug("Getting the ledstrip that we want to set a frame on.");
        LedstripProxyBase ledstrip = _ledstripContext[command.LedstripIndex];

        _logger.LogDebug("Clearing the current color on the ledstrip.");
        await _displayContext.ClearLedstripAsync(ledstrip).ConfigureAwait(false);

        _logger.LogDebug("Displaying the frame that was given to the ledstrip.");
        await _displayContext.DisplayFrameAsync(ledstrip, command.Frame).ConfigureAwait(false);

        _logger.LogDebug("Frame has been set.");
    }
}