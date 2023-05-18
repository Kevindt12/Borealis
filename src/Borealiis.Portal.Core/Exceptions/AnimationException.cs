using System;
using System.Linq;



namespace Borealis.Portal.Core.Exceptions;


public class AnimationException : ApplicationException
{
    /// <inheritdoc />
    public AnimationException() { }


    /// <inheritdoc />
    public AnimationException(String? message) : base(message) { }


    /// <inheritdoc />
    public AnimationException(String? message, Exception? innerException) : base(message, innerException) { }
}