using System;
using System.Linq;

using Borealis.Domain.Effects;
using Borealis.Portal.Core.Exceptions;
using Borealis.Portal.Domain.Animations;
using Borealis.Portal.Domain.Connectivity.Connections;
using Borealis.Portal.Domain.Effects.Factories;
using Borealis.Portal.Domain.Effects.Handlers;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Animations;


internal class AnimationPlayerFactory : IAnimationPlayerFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IEffectEngineFactory _effectEngineFactory;


    public AnimationPlayerFactory(ILoggerFactory loggerFactory, IEffectEngineFactory effectEngineFactory)
    {
        _loggerFactory = loggerFactory;
        _effectEngineFactory = effectEngineFactory;
    }


    /// <inheritdoc />
    /// <exception cref="OperationCanceledException"> When the operation ahs been cancelled by the token. </exception>
    /// <exception cref="AnimationException"> When there was a problem running the effect code. </exception>
    /// <returns> An <see cref="IAnimationPlayer" /> that is ready to be used and a animation to be played on. </returns>
    public virtual async Task<IAnimationPlayer> CreateAnimationPlayerAsync(Effect effect, ILedstripConnection ledstrip, CancellationToken token = default)
    {
        return new AnimationPlayer(_loggerFactory.CreateLogger<AnimationPlayer>(),
                                   await _effectEngineFactory.CreateEffectEngineAsync(effect, ledstrip.Ledstrip.Length, new EffectEngineOptions(), token).ConfigureAwait(false),
                                   ledstrip);
    }
}