using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Reflection;
using System.Web.UI; 
using System.Collections;
public class NotRestrictingParserFilter : PageParserFilter
{
    public override bool AllowCode
    {
        get { return true; }
    }

    public override int NumberOfControlsAllowed
    {
        get { return int.MaxValue; }
    }

    public override int NumberOfDirectDependenciesAllowed
    {
        get { return int.MaxValue; }
    }

    public override int TotalNumberOfDependenciesAllowed
    {
        get { return int.MaxValue; }
    }

    public override bool AllowBaseType(Type baseType)
    {
        return true;
    }

    public override bool AllowControl(Type controlType, ControlBuilder builder)
    {
        return true;
    }

    public override bool AllowServerSideInclude(string includeVirtualPath)
    {
        return true;
    }

    public override bool AllowVirtualReference(string referenceVirtualPath, VirtualReferenceType referenceType)
    {
        return true;
    }

    public override CompilationMode GetCompilationMode(CompilationMode current)
    {
        return base.GetCompilationMode(current);
    }

    public override Type GetNoCompileUserControlType()
    {
        return base.GetNoCompileUserControlType();
    }

    public override void PreprocessDirective(string directiveName, System.Collections.IDictionary attributes)
    {
        base.PreprocessDirective(directiveName, attributes);
    }

    public override bool ProcessCodeConstruct(CodeConstructType codeType, string code)
    {
        return base.ProcessCodeConstruct(codeType, code);
    }

    public override bool ProcessDataBindingAttribute(string controlId, string name, string value)
    {
        return base.ProcessDataBindingAttribute(controlId, name, value);
    }

    public override bool ProcessEventHookup(string controlId, string eventName, string handlerName)
    {
        return base.ProcessEventHookup(controlId, eventName, handlerName);
    }

    public override void ParseComplete(ControlBuilder rootBuilder)
    {
        base.ParseComplete(rootBuilder);
    }

    protected override void Initialize()
    {
        base.Initialize();
    }


    const BindingFlags InstPubNonpub = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    protected ControlBuilder GetParentBuilder(ControlBuilder controlBuilder)
    {
        if (controlBuilder == null)
        {
            throw new ArgumentNullException("controlBuilder");
        }

        return (ControlBuilder)controlBuilder.GetType().GetProperty("ParentBuilder", InstPubNonpub).GetValue(controlBuilder, null);
    }

    protected ControlBuilder GetRootBuilder(ControlBuilder controlBuilder)
    {
        if (controlBuilder == null)
        {
            throw new ArgumentNullException("controlBuilder");
        }

        while (GetParentBuilder(controlBuilder) != null)
        {
            controlBuilder = GetParentBuilder(controlBuilder);
        }

        return controlBuilder;
    }

    protected ArrayList GetSubBuilders(ControlBuilder controlBuilder)
    {
        if (controlBuilder == null)
        {
            throw new ArgumentNullException("controlBuilder");
        }

        return (ArrayList)controlBuilder.GetType().GetProperty("SubBuilders", InstPubNonpub).GetValue(controlBuilder, null);
    }

    protected ControlBuilder GetDefaultPropertyBuilder(ControlBuilder controlBuilder)
    {
        if (controlBuilder == null)
        {
            throw new ArgumentNullException("controlBuilder");
        }

        PropertyInfo pi = null;
        Type type = controlBuilder.GetType();

        while (type != null && (InlineAssignHelper(ref pi, type.GetProperty("DefaultPropertyBuilder", InstPubNonpub))) == null)
        {
            type = type.BaseType;
        }

        return (ControlBuilder)pi.GetValue(controlBuilder, null);
    }

    protected ArrayList GetTemplatePropertyEntries(ControlBuilder controlBuilder)
    {
        if (controlBuilder == null)
        {
            throw new ArgumentNullException("controlBuilder");
        }

        ICollection tpes = (ICollection)controlBuilder.GetType().GetProperty("TemplatePropertyEntries", InstPubNonpub).GetValue(controlBuilder, null);

        if (tpes == null || tpes.Count == 0)
        {
            return new ArrayList(0);
        }
        else
        {
            return (ArrayList)tpes;
        }
    }

    protected  ArrayList GetComplexPropertyEntries(ControlBuilder controlBuilder)
    {
        if (controlBuilder == null)
        {
            throw new ArgumentNullException("controlBuilder");
        }

        ICollection cpes = (ICollection)controlBuilder.GetType().GetProperty("ComplexPropertyEntries", InstPubNonpub).GetValue(controlBuilder, null);

        if (cpes == null || cpes.Count == 0)
        {
            return new ArrayList(0);
        }
        else
        {
            return (ArrayList)cpes;
        }
    }
    protected T InlineAssignHelper<T>(ref T target, T value)
    {
        target = value;
        return value;
    }
}