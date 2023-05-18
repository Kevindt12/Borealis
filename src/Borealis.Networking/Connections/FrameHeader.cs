namespace Borealis.Networking.Connections;


internal readonly struct FrameHeader
{
	public const int Length = 5;


	/// <summary>
	/// The type of frame that we have or want.
	/// </summary>
	public FrameType FrameType { get; }


	/// <summary>
	/// The length of the frame.
	/// </summary>
	public int ContentLength { get; }


	/// <summary>
	/// The frames that we will be sending across an connection.
	/// </summary>
	/// <param name="type"> The type of frame that we are sending. </param>
	/// <param name="length"> The length of the packet. </param>
	public FrameHeader(FrameType type, int length)
	{
		FrameType = type;
		ContentLength = length;
	}


	/// <summary>
	/// Creates a frame header indicating that this is a keep alive message.
	/// </summary>
	/// <returns> The <see cref="FrameHeader" /> that holds the frame header content. </returns>
	public static FrameHeader KeepAliveFrame()
	{
		return new FrameHeader(FrameType.KeepAlive, 0);
	}


	/// <summary>
	/// Creates a frame header indicating that this is a disconnect message.
	/// </summary>
	/// <returns> A <see cref="FrameHeader" /> that indicates that we want to start the process of disconnecting. </returns>
	public static FrameHeader DisconnectFrame()
	{
		return new FrameHeader(FrameType.KeepAlive, 0);
	}


	/// <summary>
	/// Creates the buffer header to be able to send to an remote connection.
	/// </summary>
	/// <returns> The <see cref="ReadOnlySpan{T}" /> that contains the data. </returns>
	public ReadOnlySpan<byte> CreateBuffer()
	{
		byte[] buffer = new Byte[Length];
		buffer[0] = (byte)FrameType;
		BitConverter.GetBytes(ContentLength).CopyTo(buffer, 1);

		return buffer;
	}


	/// <summary>
	/// Reads the packet header from the buffer that we want to read.
	/// </summary>
	/// <param name="buffer"> </param>
	/// <returns> </returns>
	public static FrameHeader FromBuffer(ReadOnlySpan<byte> buffer)
	{
		if (buffer.Length < 5) throw new ArgumentOutOfRangeException(nameof(buffer), "The buffer length for the packet header needs to be at least 5 bytes long.");

		FrameType type = (FrameType)buffer[0];
		int packetLength = BitConverter.ToInt32(buffer[1..5]);

		return new FrameHeader(type, packetLength);
	}
}