using System.Drawing;
using System.Text.Json;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;



namespace Borealis.Portal.Data.Converters;


public class EffectParameterValueConverter : ValueConverter<object?, string?>
{
    /// <inheritdoc />
    public EffectParameterValueConverter() : base(o => ConvertToJson(o), s => ConvertFromJson(s)) { }


    protected static string? ConvertToJson(object? value)
    {
        if (value is null) return null;

        TypedObject obj = default!;

        if (value is Color color)
        {
            obj = new TypedObject
            {
                Type = "Color",
                ObjectJson = JsonSerializer.Serialize(ColorObject.FromColor(color))
            };
        }
        else if (value is IEnumerable<Color> colors)
        {
            obj = new TypedObject
            {
                Type = "Colors",
                ObjectJson = JsonSerializer.Serialize(colors.Select(ColorObject.FromColor))
            };
        }
        else
        {
            obj = new TypedObject(value, JsonSerializer.Serialize(value));
        }

        string json = JsonSerializer.Serialize(obj);

        return json;
    }


    protected static object? ConvertFromJson(string? json)
    {
        if (json is null) return null;

        TypedObject typedObj = JsonSerializer.Deserialize<TypedObject>(json)!;

        object? result = null;

        if (typedObj.Type == "Color")
        {
            result = JsonSerializer.Deserialize<ColorObject>(typedObj.ObjectJson)!.ToColor();
        }
        else if (typedObj.Type == "Colors")
        {
            result = JsonSerializer.Deserialize<List<ColorObject>>(typedObj.ObjectJson)!.Select(x => x.ToColor()).ToList();
        }
        else
        {
            result = JsonSerializer.Deserialize(typedObj.ObjectJson, Type.GetType(typedObj.Type)!);
        }

        return result;
    }



    private record TypedObject
    {
        public TypedObject() { }


        public TypedObject(object obj, string valueJson)
        {
            Type = obj.GetType().FullName!;
            ObjectJson = valueJson;
        }


        public string Type { get; set; }

        public string ObjectJson { get; set; }
    }



    private record ColorObject
    {
        public byte R { get; set; }

        public byte G { get; set; }

        public byte B { get; set; }

        public byte A { get; set; }


        public static ColorObject FromColor(Color color)
        {
            return new ColorObject
            {
                A = color.A,
                R = color.R,
                G = color.G,
                B = color.B
            };
        }


        public Color ToColor()
        {
            return Color.FromArgb(A, R, G, B);
        }
    }
}