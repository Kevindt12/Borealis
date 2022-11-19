using System.Drawing;

using Borealis.Portal.Rpi.Contexts;
using Borealis.Portal.Rpi.Ledstrips;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;



namespace Borealis.Portal.Rpi.Services;


public class CoreService : Rpi.CoreService.CoreServiceBase
{
    private readonly LedstripContext _ledstripContext;


    public CoreService(LedstripContext ledstripContext)
    {
        _ledstripContext = ledstripContext;
    }


    /// <inheritdoc />
    public override async Task<Empty> SetFrame(IAsyncStreamReader<Frame> requestStream, ServerCallContext context)
    {
        while (await requestStream.MoveNext(context.CancellationToken))
        {
            LedstripProxyBase proxy = _ledstripContext[requestStream.Current.LedstripIndex];
            proxy.SetColors(Read3ByteArray(requestStream.Current.Data.Span).Span);

            Console.WriteLine("Recieved Data");
        }

        return new Empty();
    }


    public static Memory<Color> Read3ByteArray(ReadOnlySpan<byte> array)
    {
        Color[] colors = new Color[array.Length / 3];

        for (int i = 0; i < array.Length;)
        {
            colors[i / 3] = Color.FromArgb(i, i + 1, i + 2);

            i += 3;
        }

        return colors;
    }
}