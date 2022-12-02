using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using UnitsNet;



namespace Borealis.Portal.Data.Converters;


public class FrequencyTypeConverter : ValueConverter<Frequency, double>
{
    /// <inheritdoc />
    public FrequencyTypeConverter() : base(o => o.Hertz, s => Frequency.FromHertz(s)) { }
}