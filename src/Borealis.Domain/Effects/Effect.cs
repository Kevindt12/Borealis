using Borealis.Domain.Runtime;



namespace Borealis.Domain.Effects;


public class Effect : IEquatable<Effect>
{
    /// <summary>
    /// The id of the effect.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The javascript of the effect.
    /// </summary>
    public string Javascript { get; set; } = string.Empty;

    /// <summary>
    /// The name of the effect.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description of the effect.
    /// </summary>
    public string? Description { get; set; }


    /// <summary>
    /// The effect parameters.
    /// </summary>
    public virtual IList<EffectParameter> EffectParameters { get; set; } = new List<EffectParameter>();


    /// <summary>
    /// The Javascript modules that can be used in the javascript engine.
    /// </summary>
    public virtual ICollection<JavascriptModule> JavascriptModules { get; set; } = new List<JavascriptModule>();


    public Effect() { }


    /// <summary>
    /// A effect that can be displayed on a ledstrip.
    /// </summary>
    /// <param name="effect"> </param>
    public Effect(Effect effect)
    {
        Id = effect.Id;
        Javascript = effect.Javascript;
        Name = effect.Name;
        Description = effect.Description;
        EffectParameters = effect.EffectParameters;
    }


    /// <inheritdoc />
    public bool Equals(Effect? other)
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

        return Equals((Effect)obj);
    }


    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }


    public static bool operator ==(Effect? left, Effect? right)
    {
        return Equals(left, right);
    }


    public static bool operator !=(Effect? left, Effect? right)
    {
        return !Equals(left, right);
    }
}