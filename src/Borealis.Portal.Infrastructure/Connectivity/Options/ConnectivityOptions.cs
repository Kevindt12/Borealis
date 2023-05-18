namespace Borealis.Portal.Infrastructure.Connectivity.Options;


public class ConnectivityOptions
{
	public static string Name = "Connectivity";


	/// <summary>
	/// The receive time out for connections.
	/// </summary>
	/// <remarks>
	/// This time is in milliseconds(ms)
	/// </remarks>
	public int ReceiveTimeout { get; set; } = 10000;


	/// <summary>
	/// The time we wait for an response from the driver.
	/// </summary>
	/// <remarks>
	/// This time is in milliseconds(ms)
	/// </remarks>
	public int ResponseTimeout { get; set; } = 10000;
}