namespace Borealis.Domain.Effects;


public class EffectFile
{
    private string _javascript;

    /// <summary>
    /// The id of the file.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The date time of its saving.
    /// </summary>
    public DateTime Updated { get; set; }

    /// <summary>
    /// The javascript code.
    /// </summary>
    public string Javascript
    {
        get => _javascript;
        set
        {
            _javascript = value;
            Updated = DateTime.UtcNow;
        }
    }


    public EffectFile()
    {
        Updated = DateTime.UtcNow;
    }


    public EffectFile(string code)
    {
        Updated = DateTime.UtcNow;
        Javascript = code;
    }
}