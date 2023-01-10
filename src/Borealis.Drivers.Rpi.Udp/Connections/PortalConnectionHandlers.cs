using Borealis.Domain.Devices;
using Borealis.Domain.Effects;

using UnitsNet;



namespace Borealis.Drivers.Rpi.Udp.Connections;


public class PortalConnectionHandlers
{
    public required ConnectionCallback ConnectionCallback { get; init; }


    public required StartAnimationCallback StartAnimationCallback { get; init; }

    public required StopAnimationCallback StopAnimationCallback { get; init; }

    public required HandleSingleFrameCallback HandleSingleFrameCallback { get; init; }

    public required HandleFrameBufferCallback HandleFrameBufferCallback { get; init; }

    public required HandleConfigurationCallback HandleConfigurationCallback { get; init; }
}



/// <summary>
/// </summary>
/// <param name="configurationConcurrencyToken"> </param>
/// <returns> A bool indicating that the concurrency token is valid. </returns>
public delegate Task<bool> ConnectionCallback(string configurationConcurrencyToken);



public delegate Task StartAnimationCallback(byte ledstripIndex, Frequency frequency, IEnumerable<ReadOnlyMemory<PixelColor>> initialFrames);



public delegate Task StopAnimationCallback(byte ledstripIndex);



public delegate Task HandleSingleFrameCallback(byte ledstripIndex, ReadOnlyMemory<PixelColor> frame);



public delegate Task HandleFrameBufferCallback(byte ledstripIndex, IEnumerable<ReadOnlyMemory<PixelColor>> frames);



public delegate Task HandleConfigurationCallback(LedstripSettings settings);