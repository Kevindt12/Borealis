using System;
using System.Linq;



namespace Borealis.Drivers.Rpi.Options;


public class AnimationOptions
{
    public const string Name = "Animation";


    /// <summary>
    /// The target stack size that we want when playing animations.
    /// </summary>
    public int TargetStackSize { get; set; } = 500;


    public double FrameBufferRequestThreshold { get; set; } = 0.8;
}