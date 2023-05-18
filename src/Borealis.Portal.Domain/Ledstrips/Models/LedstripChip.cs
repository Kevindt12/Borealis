// ReSharper disable InconsistentNaming


using System;
using System.Linq;



namespace Borealis.Portal.Domain.Ledstrips.Models;


/// <summary>
/// Gets the type of chip that is used on the ledstrip.
/// </summary>
public enum LedstripChip
{
    WS2812B = 1,
    WS2813 = 2,
    WS2815 = 3,
    APA102 = 4,
    SK6812 = 5,
    SK9822 = 6
}