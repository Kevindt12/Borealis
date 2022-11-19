using System.Text;



namespace Borealis.Portal.Web.Utilities;


public class CssBuilder
{
	private readonly StringBuilder _sb;


	public CssBuilder()
	{
		_sb = new StringBuilder();
	}


	public CssBuilder(string @class)
	{
		_sb = new StringBuilder(@class);
	}


	public CssBuilder AddClass(string @class, bool flag = true)
	{
		if (flag)
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