using Borealis.Portal.Domain.Devices;



namespace Borealis.Portal.Domain.Exceptions;


public class InvalidDeviceConfigurationException : InvalidOperationException
{
	public Device? Device { get; set; }


	/// <inheritdoc />
	public InvalidDeviceConfigurationException(String? message, Exception? innerException) : base(message, innerException) { }


	/// <inheritdoc />
	public InvalidDeviceConfigurationException(String? message) : base(message) { }
}