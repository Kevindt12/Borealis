using Borealis.Domain.Effects;
using Borealis.Portal.Domain.Effects.Options;

using Microsoft.Extensions.Options;



namespace Borealis.Portal.Domain.Effects.Factories;


internal class EffectFactory : IEffectFactory
{
    private readonly JavascriptFilePathsOptions _javascriptFilePathsOptions;


    public EffectFactory(IOptionsSnapshot<JavascriptFilePathsOptions> javascriptFilesOptions)
    {
        _javascriptFilePathsOptions = javascriptFilesOptions.Value;
    }


    /// <inheritdoc />
    public Effect CreateEffect(string effectName)
    {
        string fileContent = File.ReadAllText(_javascriptFilePathsOptions.EffectFileBoilerplatePath);

        Effect effect = new Effect();
        effect.Name = effectName;
        effect.Files.Add(new EffectFile(fileContent));

        return effect;
    }
}