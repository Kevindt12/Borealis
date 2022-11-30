using Borealis.Domain.Effects;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Effects;


public class EffectEngineFactory
{
    private readonly ILoggerFactory _loggerFactory;


    public EffectEngineFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }


    public EffectEngine CreateEffectEngine(Effect effect, int ledstripLength, EffectEngineOptions options = null)
    {
        return new EffectEngine(_loggerFactory.CreateLogger<EffectEngine>(), effect, ledstripLength, options);
    }
}