using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.Configuration;
public enum WhiteSpaceCleaning
{
    None,
    Clean,
    CleanOnReleaseBuild
}

public class MinifyPageParserFilter : NotRestrictingParserFilter
{
    public MinifyPageParserFilter()
        : base()
    {
    }

    const string WhiteSpaceDirectiveName = "whitespacecleaning";

    private System.Nullable<WhiteSpaceCleaning> _whiteSpaceCleaning = null;

    public override void PreprocessDirective(string directiveName, System.Collections.IDictionary attributes)
    {
        if (attributes.Contains(WhiteSpaceDirectiveName))
        {
            _whiteSpaceCleaning = (WhiteSpaceCleaning)Enum.Parse(typeof(WhiteSpaceCleaning), (string)attributes[WhiteSpaceDirectiveName]);
            attributes.Remove(WhiteSpaceDirectiveName);
        }

        base.PreprocessDirective(directiveName, attributes);
    }

    public override void ParseComplete(ControlBuilder rootBuilder)
    {
        WhiteSpaceCleaning whiteSpace = (_whiteSpaceCleaning.HasValue ? _whiteSpaceCleaning.Value : WhiteSpaceCleaning.Clean);

        if (whiteSpace == WhiteSpaceCleaning.Clean || (whiteSpace == WhiteSpaceCleaning.CleanOnReleaseBuild && !IsDebuggingEnabled()))
        {
            TrimStringSubBuilders(rootBuilder);
        }

        base.ParseComplete(rootBuilder);
    }

    private void TrimStringSubBuilders(ControlBuilder controlBuilder)
    {
        ArrayList subBuilders = GetSubBuilders(controlBuilder);

        for (int i = 0; i <= subBuilders.Count - 1; i++)
        {
            string literal = subBuilders[i] as string;

            if (string.IsNullOrEmpty(literal))
            {
                continue;
            }

            subBuilders[i] = CleanWhiteSpace(literal);
        }

        foreach (object subBuilder in subBuilders)
        {
            if (subBuilder is ControlBuilder)
            {
                TrimStringSubBuilders((ControlBuilder)subBuilder);
            }
        }

        foreach (TemplatePropertyEntry entry in GetTemplatePropertyEntries(controlBuilder))
        {
            TrimStringSubBuilders(entry.Builder);
        }

        foreach (ComplexPropertyEntry entry in GetComplexPropertyEntries(controlBuilder))
        {
            TrimStringSubBuilders(entry.Builder);
        }

        ControlBuilder defaultPropertyBuilder = GetDefaultPropertyBuilder(controlBuilder);

        if (defaultPropertyBuilder != null)
        {
            TrimStringSubBuilders(defaultPropertyBuilder);
        }
    }

    private string CleanWhiteSpace(string literal)
    {
        return HTMLMin.HtmlTidy(literal, true, true, true);
    }

    private bool IsDebuggingEnabled()
    {
        if (HttpContext.Current != null)
        {
            return HttpContext.Current.IsDebuggingEnabled;
        }

        return (bool)((CompilationSection)WebConfigurationManager.GetSection("system.web/compilation", VirtualPath)).Debug;
    }
}