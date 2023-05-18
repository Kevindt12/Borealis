using Borealis.Drivers.RaspberryPi.Sharp.Connection.Controllers;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Manager;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Validation;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Service;

using Microsoft.Extensions.Logging;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Factories;


public class ConnectionControllerFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDeviceConfigurationValidator _deviceConfigurationValidator;
    private readonly ILedstripConfigurationService _ledstripConfigurationService;
    private readonly ILedstripControlService _ledstripControlService;
    private readonly IDeviceConfigurationManager _deviceConfigurationManager;


    public ConnectionControllerFactory(ILoggerFactory loggerFactory,
                                       IDeviceConfigurationValidator deviceConfigurationValidator,
                                       ILedstripConfigurationService ledstripConfigurationService,
                                       ILedstripControlService ledstripControlService,
                                       IDeviceConfigurationManager deviceConfigurationManager)
    {
        _loggerFactory = loggerFactory;
        _deviceConfigurationValidator = deviceConfigurationValidator;
        _ledstripConfigurationService = ledstripConfigurationService;
        _ledstripControlService = ledstripControlService;
        _deviceConfigurationManager = deviceConfigurationManager;
    }


    public ConnectionController Create()
    {
        return new ConnectionController(_loggerFactory.CreateLogger<ConnectionController>(), _deviceConfigurationValidator, _ledstripControlService, _ledstripConfigurationService, _deviceConfigurationManager);
    }
}