using System;
using System.Linq;

using Borealis.Portal.Domain.Ledstrips.Models;



namespace Borealis.Domain.Ledstrips;


public class Ledstrip
{
    /// <summary>
    /// The id of the ledstrip.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The number of pixels on the ledstrip.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// The chip that is used on the ledstrip.
    /// </summary>
    public LedstripChip Chip { get; set; }

    /// <summary>
    /// The name of the ledstrip.
    /// </summary>
    public string? Name { get; set; }


    /// <summary>
    /// The ledstrip that we can use to display something.
    /// </summary>
    public Ledstrip() { }


    /// <inheritdoc cref="Ledstrip" />
    /// <param name="name"> The name of the ledstrip. </param>
    /// <param name="length"> The length of the ledstrip. </param>
    /// <param name="chip"> What chip the ledstrip uses. </param>
    public Ledstrip(string? name, int length, LedstripChip chip)
    {
        Id = Guid.NewGuid();
        Length = length;
        Chip = chip;
        Name = name;
    }


    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name} ({Length})";
    }
}