using System.Net;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;



namespace Borealis.Portal.Data.Converters;


public class IPEndPointTypeConverter : ValueConverter<IPEndPoint?, string?>
{
    /// <inheritdoc />
    public IPEndPointTypeConverter() : base(o => ConvertToString(o), s => ConvertToIPEndPoint(s)) { }


    protected static string? ConvertToString(IPEndPoint? value)
    {
        if (value is null) return null;

        return value.ToString();
    }


    protected static IPEndPoint? ConvertToIPEndPoint(string? endPoint)
    {
        if (endPoint is null) return null;

        return IPEndPoint.Parse(endPoint);
    }
}