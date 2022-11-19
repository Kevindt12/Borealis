using System;
using System.Linq;

using Borealis.Domain.Devices;
using Borealis.Domain.Effects;
using Borealis.Portal.Infrastructure.Connections;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Interaction;


internal class SolidColorInteractorFactory
{
    private readonly ILoggerFactory _loggerFactory;


    public SolidColorInteractorFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }


    public SolidColorInteractor CreateSolidColorInteractor(IDeviceConnection connection, Ledstrip ledstrip, PixelColor color)
    {
        return new SolidColorInteractor(_loggerFactory.CreateLogger<SolidColorInteractor>(), connection, ledstrip, color);
    }
}