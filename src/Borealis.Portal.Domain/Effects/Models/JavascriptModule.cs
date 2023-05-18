namespace Borealis.Domain.Runtime;


public class JavascriptModule
{
    /// <summary>
    /// The Id of the module.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the module.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The code that we want to
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    /// The version of the javascript module.
    /// </summary>
    public string? Version { get; set; } = string.Empty;


    public JavascriptModule() { }


    /// <summary>
    /// A javascript modules that can be loaded in the effect engine.
    /// </summary>
    public JavascriptModule(string code)
    {
        Id = Guid.Empty;
        Code = code;
    }
}