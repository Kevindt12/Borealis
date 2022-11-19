using System;
using System.Linq;

using Borealis.Domain.Animations;



namespace Borealis.Portal.Core.Exceptions;


public class AnimationException : ApplicationException
{
    public Animation Animation { get; set; }


    /// <inheritdoc />
    public AnimationException(Animation animation)
    {
        Animation = animation;
    }


    /// <inheritdoc />
    public AnimationException(String? message, Animation animation) : base(message)
    {
        Animation = animation;
    }


    /// <inheritdoc />
    public AnimationException(String? message, Exception? innerException, Animation animation) : base(message, innerException)
    {
        Animation = animation;
    }


    /// <inheritdoc />
    public AnimationException() { }


    /// <inheritdoc />
    public AnimationException(String? message) : base(message) { }


    /// <inheritdoc />
    public AnimationException(String? message, Exception? innerException) : base(message, innerException) { }
}