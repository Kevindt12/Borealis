namespace Borealis.Portal.Domain.Devices.Models;


public enum LedstripStatus
{
    /// <summary>
    /// The ledstrip is not busy and all actions can be taken to start anything on the ledstrip.
    /// </summary>
    Idle = 0,

    /// <summary>
    /// The ledstrip is busy displaying a single color.
    /// </summary>
    DisplayingColor = 1,

    /// <summary>
    /// The ledstrip is busy playing an animation.
    /// </summary>
    PlayingAnimation = 2,

    /// <summary>
    /// The ledstrip was playing an animation and its still loaded but its paused.
    /// </summary>
    PausedAnimation = 3
}