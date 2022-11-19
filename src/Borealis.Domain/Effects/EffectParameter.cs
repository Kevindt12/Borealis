using System.Drawing;



namespace Borealis.Domain.Effects;


/// <summary>
/// A parameter for a effect.
/// </summary>
public class EffectParameter : IEquatable<EffectParameter>
{
    /// <summary>
    /// The id of the effect parameter.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The identifier that can be used in the javascript.
    /// </summary>
    public string Identifier { get; set; } = String.Empty;

    /// <summary>
    /// Description of the effect.
    /// </summary>
    public string Description { get; set; } = String.Empty;

    /// <summary>
    /// The effect parameter type.
    /// </summary>
    public EffectParameterType ParameterType { get; set; }

    /// <summary>
    /// The value of the effect parameter.
    /// </summary>
    /// <remarks>
    /// This value is boxed and should support null.
    /// </remarks>
    public object? Value { get; set; }


    public EffectParameter() { }


    protected EffectParameter(EffectParameterType type, object? value)
    {
        ParameterType = type;
        Value = value;
    }


    /// <summary>
    /// Initializes a new Effect Parameter.
    /// </summary>
    /// <param name="startingType"> The underlying type. </param>
    /// <returns>
    /// A <see cref="EffectParameter" /> ready to be used or added to
    /// <see cref="Effect" />.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <see cref="startingType" />
    /// ios out of range.
    /// </exception>
    public static EffectParameter New(EffectParameterType startingType)
    {
        return startingType switch
        {
            EffectParameterType.Text       => new EffectParameter(EffectParameterType.Text, String.Empty),
            EffectParameterType.Number     => new EffectParameter(EffectParameterType.Number, 0.0d),
            EffectParameterType.Boolean    => new EffectParameter(EffectParameterType.Boolean, false),
            EffectParameterType.Color      => new EffectParameter(EffectParameterType.Color, Color.Black),
            EffectParameterType.ColorArray => new EffectParameter(EffectParameterType.ColorArray, new List<Color>()),
            _                              => throw new ArgumentOutOfRangeException(nameof(startingType))
        };
    }


    /// <inheritdoc />
    public bool Equals(EffectParameter? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Id.Equals(other.Id);
    }


    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((EffectParameter)obj);
    }


    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }


    public static bool operator ==(EffectParameter? left, EffectParameter? right)
    {
        return Equals(left, right);
    }


    public static bool operator !=(EffectParameter? left, EffectParameter? right)
    {
        return !Equals(left, right);
    }
}