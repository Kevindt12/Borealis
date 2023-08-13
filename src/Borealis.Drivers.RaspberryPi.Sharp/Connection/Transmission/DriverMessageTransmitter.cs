using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Manager;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Validation;
using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Service;
using Borealis.Networking.Messages;
using Borealis.Networking.Protocol;
using Borealis.Networking.Transmission;
using Borealis.Shared.Extensions;

using Microsoft.Extensions.Logging;

using UnitsNet;

using LedstripChip = Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models.LedstripChip;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Transmission;


public sealed class DriverMessageTransmitter : MessageTransmitterBase
{
	private readonly ILogger<DriverMessageTransmitter> _logger;

	private readonly IDeviceConfigurationValidator _deviceConfigurationValidator;
	private readonly ILedstripControlService _ledstripControlService;
	private readonly ILedstripConfigurationService _ledstripConfigurationService;
	private readonly IDeviceConfigurationManager _deviceConfigurationManager;


	/// <summary>
	/// The core message transmitter for this application.
	/// </summary>
	public DriverMessageTransmitter(ILogger<DriverMessageTransmitter> logger,
									IChannel channel,
									IDeviceConfigurationValidator deviceConfigurationValidator,
									ILedstripControlService ledstripControlService,
									ILedstripConfigurationService ledstripConfigurationService,
									IDeviceConfigurationManager deviceConfigurationManager) : base(logger, channel)
	{
		_logger = logger;
		_deviceConfigurationValidator = deviceConfigurationValidator;
		_ledstripControlService = ledstripControlService;
		_ledstripConfigurationService = ledstripConfigurationService;
		_deviceConfigurationManager = deviceConfigurationManager;
	}


	/// <inheritdoc />
	protected override async Task<ConnectReply> HandleConnectRequestAsync(ConnectRequest request, CancellationToken token)
	{
		// Getting the token logging and returning.
		_logger.LogInformation("Starting connection process with portal.");
		DeviceConfiguration deviceConfiguration = await _deviceConfigurationManager.GetDeviceLedstripConfigurationAsync(token).ConfigureAwait(false);

		// Checking concurrency token.
		string driverToken = deviceConfiguration.ConcurrencyToken;
		_logger.LogInformation($"Checking configuration token: {driverToken}");

		return new ConnectReply
		{
			ConcurrencyToken = driverToken
		};
	}


	/// <inheritdoc />
	protected override async Task<SuccessReply> HandleSetConfigurationRequestAsync(SetConfigurationRequest request, CancellationToken token)
	{
		_logger.LogInformation("Request to set configuration has come in. Updating ledstrip configuration.");

		// Creating the ledstrips.
		List<Ledstrip> ledstrips = request.ConfigurationMessage.Ledstrips.Select(x => new Ledstrip
										   {
											   Bus = Convert.ToByte(x.BusId),
											   Chip = ConvertLedstripChip(x.Chip),
											   Id = x.LedstripId.ToGuid(),
											   PixelCount = Convert.ToUInt16(x.PixelCount)
										   })
										  .ToList();

		// Creating the configuration.
		DeviceConfiguration configuration = new DeviceConfiguration
		{
			ConcurrencyToken = request.ConcurrencyToken,
			Ledstrips = new List<Ledstrip>(ledstrips)
		};

		// Validating the configuration.
		_logger.LogTrace($"Validating device configuration, configuration : {configuration.GenerateLogMessage()}");
		DeviceConfigurationValidationResult result = await _deviceConfigurationValidator.ValidateAsync(configuration, token).ConfigureAwait(false);

		if (!result.Successful) throw new ValidationException("The device configuration was not valid", result.Errors);

		// Setting the new configuration.
		await _ledstripConfigurationService.LoadConfigurationAsync(configuration).ConfigureAwait(false);
		await _deviceConfigurationManager.UpdateDeviceLedstripConfigurationAsync(configuration, token).ConfigureAwait(false);

		return Success();
	}


	/// <inheritdoc />
	protected override async Task<SuccessReply> HandleStartAnimationRequestAsync(StartAnimationRequest request, CancellationToken token)
	{
		_logger.LogInformation("Request to start animation on ledstrip came in.");
		Ledstrip ledstrip = _ledstripControlService.GetLedstripById(request.LedstripId.ToGuid()) ?? throw new LedstripNotFoundException("No ledstrip with id found.");

		await _ledstripControlService.StartAnimationAsync(ledstrip, Frequency.FromHertz(request.Frequency), request.InitialFrameBuffer.Select(ConvertFrameMessage).ToArray(), token).ConfigureAwait(false);

		return new SuccessReply();
	}


	/// <inheritdoc />
	protected override async Task<SuccessReply> HandleStopAnimationRequestAsync(StopAnimationRequest request, CancellationToken token)
	{
		_logger.LogInformation("Request to stop animation has come in.");
		Ledstrip ledstrip = _ledstripControlService.GetLedstripById(request.LedstripId.ToGuid()) ?? throw new LedstripNotFoundException("No ledstrip with id found.");

		await _ledstripControlService.StopAnimationAsync(ledstrip, token).ConfigureAwait(false);

		return Success();
	}


	/// <inheritdoc />
	protected override async Task<SuccessReply> HandleDisplayFrameRequestAsync(DisplayFrameRequest request, CancellationToken token)
	{
		_logger.LogInformation("Request ot display frame on ledstrip has come in.");
		Ledstrip ledstrip = _ledstripControlService.GetLedstripById(request.LedstripId.ToGuid()) ?? throw new InvalidOperationException("No ledstrip with id found.");

		await _ledstripControlService.DisplayFameAsync(ledstrip, ConvertFrameMessage(request.Frame), token).ConfigureAwait(false);

		return Success();
	}


	/// <inheritdoc />
	protected override async Task<SuccessReply> HandleClearLedstripRequestAsync(ClearLedstripRequest request, CancellationToken token)
	{
		_logger.LogInformation("Request to clear the animation on the ledstrip.");
		Ledstrip ledstrip = _ledstripControlService.GetLedstripById(request.LedstripId.ToGuid()) ?? throw new LedstripNotFoundException("No ledstrip with id found.");

		await _ledstripControlService.ClearLedstripAsync(ledstrip, token).ConfigureAwait(false);

		return Success();
	}


	private LedstripChip ConvertLedstripChip(Networking.Messages.LedstripChip chip) => chip switch { };


	public static ReadOnlyMemory<PixelColor> ConvertFrameMessage(FrameMessage frameMessage)
	{
		PixelColor[] pixelColors = new PixelColor[frameMessage.Pixels.Count];

		for (var i = 0; i < frameMessage.Pixels.Count; i++)
		{
			byte[] pixelEncoded = BitConverter.GetBytes(frameMessage.Pixels[i]);

			pixelColors[i] = new PixelColor
			{
				R = pixelEncoded[0], G = pixelEncoded[1], B = pixelEncoded[2], W = pixelEncoded[3]
			};
		}

		return pixelColors;
	}
}