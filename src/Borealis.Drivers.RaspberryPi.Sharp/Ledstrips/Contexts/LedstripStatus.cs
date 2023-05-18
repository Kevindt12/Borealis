namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Contexts;


public enum LedstripStatus
{
    Idle = 0,

    DisplayingFrame = 1,

    PlayingAnimation = 2,

    PausedAnimation = 3
}