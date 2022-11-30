using System.Text;



namespace Borealis.Portal.Web.Utilities;


public class CssClassBuilder
{
    private readonly StringBuilder _sb;


    public CssClassBuilder()
    {
        _sb = new StringBuilder();
    }


    public CssClassBuilder(string @class)
    {
        _sb = new StringBuilder(@class);
    }


    public CssClassBuilder AddClass(string @class, bool enableFlag = true)
    {
        if (enableFlag)
        {
            _sb.Append(@class);
            _sb.Append(' ');
        }

        return this;
    }


    public string Build()
    {
        return _sb.ToString();
    }
}