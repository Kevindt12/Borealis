namespace Borealis.Portal.Infrastructure.Connectivity.Options;


public class KeepAliveOptions
{
	public static string Name = "KeepAlive";

	/// <summary>
	/// How long we should wait until we send a other an keep alive message.
	/// </summary>
	/// <remarks>
	/// Json format : "00:00:00.000"
	/// </remarks>
	public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(10);

	/// <summary>
	/// How many times we retry before we throw the exception that we cannot read from the socket anymore.
	/// </summary>
	public int RetryCount { get; set; } = 3;
}