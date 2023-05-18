namespace Borealis.Portal.Domain.Effects.Options;


public class JavascriptFilePathsOptions
{
    public const string Name = "JavascriptPaths";


    /// <summary>
    /// The path of the base library used in the effect engine.
    /// </summary>
    public string JavascriptBaseLibraryPath { get; set; }


    /// <summary>
    /// The path of the boilerplate effect file.
    /// </summary>
    public string EffectFileBoilerplatePath { get; set; }
}