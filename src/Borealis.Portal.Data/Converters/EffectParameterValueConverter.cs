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

        TypedObject obj = new TypedObject(value, JsonSerializer.Serialize(value));
        string json = JsonSerializer.Serialize(obj);

        return json;
    }


    protected static object? ConvertFromJson(string? json)
    {
        if (json is null) return null;

        TypedObject typedObj = JsonSerializer.Deserialize<TypedObject>(json)!;

        object? result = JsonSerializer.Deserialize(typedObj.ObjectJson, Type.GetType(typedObj.Type)!);

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
}