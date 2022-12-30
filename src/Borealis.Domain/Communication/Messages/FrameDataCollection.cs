using System.Collections.ObjectModel;



namespace Borealis.Domain.Communication.Messages;


public class FrameDataCollection : Collection<FrameData>
{
    public int FrameSize => Items.FirstOrDefault()?.ByteLength ?? 0;

    public FrameDataCollection() { }

    public FrameDataCollection(IEnumerable<FrameData> frames) { }

    public FrameData this[int index]
    {
        get
        {
            return Items[index];
        }
        set
        {
            Items[index] = value;
        }
    }
}