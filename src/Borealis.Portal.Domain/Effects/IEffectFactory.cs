using Borealis.Domain.Effects;



namespace Borealis.Portal.Domain.Effects;


public interface IEffectFactory
{
	Effect CreateEffect(string effectName);
}