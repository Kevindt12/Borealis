using System;
using System.Linq;

using Borealis.Domain.Animations;
using Borealis.Portal.Core.Effects;
using Borealis.Portal.Domain.Animations;
using Borealis.Portal.Domain.Connections;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Animations;


internal class AnimationPlayerFactory : IAnimationPlayerFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly EffectEngineFactory _effectEngineFactory;


    public AnimationPlayerFactory(ILoggerFactory loggerFactory, EffectEngineFactory effectEngineFactory)
    {
        _loggerFactory = loggerFactory;
        _effectEngineFactory = effectEngineFactory;
    }


    /// <inheritdoc />
    public IAnimationPlayer CreateAnimationPlayer(Animation animation, ILedstripConnection ledstrip)
    {
        return new AnimationPlayer(_loggerFactory.CreateLogger<AnimationPlayer>(),
                                   animation,
                                   _effectEngineFactory.CreateEffectEngine(animation.Effect, ledstrip.Ledstrip.Length),
                                   ledstrip);
    }
}