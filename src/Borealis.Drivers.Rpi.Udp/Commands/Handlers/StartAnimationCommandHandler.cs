using Borealis.Drivers.Rpi.Udp.Commands.Actions;
using Borealis.Drivers.Rpi.Udp.Contexts;
using Borealis.Drivers.Rpi.Udp.Ledstrips;



namespace Borealis.Drivers.Rpi.Udp.Commands.Handlers;


public class StartAnimationCommandHandler : ICommandHandler<StartAnimationCommand>
{
    private readonly ILogger<StartAnimationCommandHandler> _logger;
    private readonly LedstripContext _ledstripContext;
    private readonly DisplayContext _displayContext;


    public StartAnimationCommandHandler(ILogger<StartAnimationCommandHandler> logger, LedstripContext ledstripContext, DisplayContext displayContext)
    {
        _logger = logger;
        _ledstripContext = ledstripContext;
        _displayContext = displayContext;
    }


    /// <inheritdoc />
    public async Task ExecuteAsync(StartAnimationCommand command)
    {
        // Getting the ledstrip that we want to set a frame on.
        _logger.LogDebug("Getting the ledstrip that we want to set a frame on.");
        LedstripProxyBase ledstrip = _ledstripContext[command.LedstripIndex];

        _logger.LogDebug("Displaying the frame that was given to the ledstrip.");
        await _displayContext.StartAnimationAsync(ledstrip, command.Frequency, command.InitialFrameBuffer).ConfigureAwait(false);

        _logger.LogDebug("Frame has been set.");
    }
}