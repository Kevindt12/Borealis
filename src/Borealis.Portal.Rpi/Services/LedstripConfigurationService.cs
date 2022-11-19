using Google.Protobuf.WellKnownTypes;

using Grpc.Core;



namespace Borealis.Portal.Rpi.Services;


public class LedstripConfigurationService : Rpi.LedstripConfigurationService.LedstripConfigurationServiceBase
{
	/// <inheritdoc />
	public override Task<Configuration> GetConfiguration(Empty request, ServerCallContext context)
	{
		throw new NotImplementedException();
	}


	/// <inheritdoc />
	public override Task<Empty> SetConfiguration(Configuration request, ServerCallContext context)
	{
		throw new NotImplementedException();
	}
}