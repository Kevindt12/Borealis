using Borealis.Domain.Effects;

using UnitsNet;



namespace Borealis.Drivers.Rpi.Udp.Buffering;


public class FrameBuffer : Stack<ReadOnlyMemory<PixelColor>>
{
    /// <summary>
    /// The size of the buffers.
    /// </summary>
    /// <remarks>
    /// Indicating that how many frames are the target.
    /// </remarks>
    public int Size { get; set; }

    /// <summary>
    /// The size of the buffers.
    /// </summary>
    public Information Information { get; set; }


    public FrameBuffer(int intialSize) { }
}