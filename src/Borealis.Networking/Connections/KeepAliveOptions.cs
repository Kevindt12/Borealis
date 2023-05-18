namespace Borealis.Networking.Connections;


public class KeepAliveOptions
{
	/// <summary>
	/// If we should enable the keep alive feature.
	/// </summary>
	public bool Enable { get; init; }

	/// <summary>
	/// The time we should wait before we want to know that the connection is still up.
	/// </summary>
	public TimeSpan MaxIdleTime { get; init; }


	/// <summary>
	/// The options that dictates the functionality of the keep alive feature.
	/// </summary>
	public KeepAliveOptions() { }
}