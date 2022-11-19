using System;
using System.Linq;

using Borealis.Domain.Devices;
using Borealis.Domain.Effects;
using Borealis.Portal.Infrastructure.Connections;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Interaction;


internal class SolidColorInteractor : LedstripInteractorBase
{
    private readonly PixelColor _color;


    public SolidColorInteractor(ILogger<SolidColorInteractor> logger, IDeviceConnection connection, Ledstrip ledstrip, PixelColor color) : base(logger, connection, ledstrip)
    {
        _color = color;
    }


    /// <inheritdoc />
    protected override async Task OnStartAsync(CancellationToken token)
    {
        await SendColors(Enumerable.Repeat(_color, Ledstrip.Length).ToArray());
    }
}