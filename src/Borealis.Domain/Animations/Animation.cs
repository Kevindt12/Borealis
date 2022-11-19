namespace Borealis.Domain.Animations;


public class Animation : IEquatable<Animation>
{
    /// <summary>
    /// The id of the animation.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the animation.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The duration of the effect.
    /// </summary>
    public TimeSpan TimeSpan { get; set; }

    /// <summary>
    /// The frequency in hertz.
    /// </summary>
    public int Frequency { get; set; }

    /// <summary>
    /// The effect of the animation.
    /// </summary>
    public AnimationEffect Effect { get; set; }


    /// <summary>
    /// A animation of a effect that can be played.
    /// </summary>
    public Animation() { }


    /// <summary>
    /// A animation of a effect that can be played.
    /// </summary>
    public Animation(string name, AnimationEffect effect)
    {
        Name = name;
        Effect = effect;
    }


    /// <inheritdoc />
    public bool Equals(Animation? other)
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

        return Equals((Animation)obj);
    }


    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }


    public static bool operator ==(Animation? left, Animation? right)
    {
        return Equals(left, right);
    }


    public static bool operator !=(Animation? left, Animation? right)
    {
        return !Equals(left, right);
    }
}