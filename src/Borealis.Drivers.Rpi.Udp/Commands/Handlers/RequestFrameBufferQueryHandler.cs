using System;
using System.Linq;

using Borealis.Drivers.Rpi.Commands.Actions;
using Borealis.Drivers.Rpi.Connections;
using Borealis.Drivers.Rpi.Contexts;
using Borealis.Drivers.Rpi.Exceptions;



namespace Borealis.Drivers.Rpi.Commands.Handlers;


public class RequestFrameBufferQueryHandler : IQueryHandler<RequestFrameBufferCommand, FrameBufferQuery>
{
    private readonly ILogger<RequestFrameBufferQueryHandler> _logger;
    private readonly ConnectionContext _connectionContext;
    private readonly LedstripContext _ledstripContext;


    public RequestFrameBufferQueryHandler(ILogger<RequestFrameBufferQueryHandler> logger, ConnectionContext connectionContext, LedstripContext ledstripContext)
    {
        _logger = logger;
        _connectionContext = connectionContext;
        _ledstripContext = ledstripContext;
    }


    /// <inheritdoc />
    public async Task<FrameBufferQuery> Execute(RequestFrameBufferCommand command)
    {
        // Checking if the portal is still connected.
        if (_connectionContext.Connection == null) throw new PortalConnectionException("The portal is disconnected.");
        PortalConnection connection = _connectionContext.Connection;

        // Getting the ledstrip index.
        byte ledstripIndex = _ledstripContext.IndexOf(command.LedstripProxyBase);

        try
        {
            // Getting the frame buffer from the portal.
            ReadOnlyMemory<PixelColor>[] frameBuffer = await connection.SendRequestForFrameBuffer(ledstripIndex, command.Amount).ConfigureAwait(false);

            return new FrameBufferQuery
            {
                Frames = frameBuffer
            };
        }
        catch (PortalException e)
        {
            _logger.LogError("There was a error while getting a frame buffer from the portal.");

            throw;
        }
    }
}