using Borealis.Domain.Effects;



namespace Borealis.Portal.Domain.Effects;


internal class EffectFactory : IEffectFactory
{
    /// <inheritdoc />
    public Effect CreateEffect(String effectName)
    {
        return new Effect
            { Name = effectName, Javascript = BoilerplateJavascript() };
    }


    protected virtual string BoilerplateJavascript()
    {
        return """"

		function setup() {
		  
		}

		function loop() {
		  
		}
		
		"""";
    }
}