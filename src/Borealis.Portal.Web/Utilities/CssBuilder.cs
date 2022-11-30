﻿using System.Text;



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


    public CssBuilder AddStyle(string property, string? value, bool enableFlag)
    {
        if (!enableFlag) return this;

        _sb.Append(property);
        _sb.Append(':');
        _sb.Append(value);
        _sb.Append("; ");

        return this;
    }


    public string Build()
    {
        return _sb.ToString();
    }
}