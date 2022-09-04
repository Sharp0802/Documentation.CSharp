using System.Reflection;

namespace Documentation.CSharp.Compiler;

public static class AccessibilityHelper
{
    public static Accessibility GetAccessibility(MethodBase method)
    {
        if (method.IsPrivate)
            return Accessibility.Private;
        else if (method.IsFamilyAndAssembly)
            return Accessibility.PrivateProtected;
        else if (method.IsFamily)
            return Accessibility.Protected;
        else if (method.IsFamilyOrAssembly)
            return Accessibility.ProtectedInternal;
        else if (method.IsAssembly)
            return Accessibility.Internal;
        else if (method.IsPublic)
            return Accessibility.Public;
        else 
            return Accessibility.None;
    }

    public static Accessibility GetAccessibility(FieldInfo field)
    {
        if (field.IsPrivate)
            return Accessibility.Private;
        else if (field.IsFamilyAndAssembly)
            return Accessibility.PrivateProtected;
        else if (field.IsFamily)
            return Accessibility.Protected;
        else if (field.IsFamilyOrAssembly)
            return Accessibility.ProtectedInternal;
        else if (field.IsAssembly)
            return Accessibility.Internal;
        else if (field.IsPublic)
            return Accessibility.Public;
        else 
            return Accessibility.None;
    }
    
    public static Accessibility GetAccessibility(Type type)
    {
        var visibility = type.Attributes & TypeAttributes.VisibilityMask;
        
        if (visibility == TypeAttributes.NotPublic)
            return Accessibility.Private;
        else if (visibility is TypeAttributes.NestedFamANDAssem)
            return Accessibility.PrivateProtected;
        else if (visibility is TypeAttributes.NestedFamily)
            return Accessibility.Protected;
        else if (visibility is TypeAttributes.NestedFamORAssem)
            return Accessibility.ProtectedInternal;
        else if (visibility is TypeAttributes.NestedAssembly)
            return Accessibility.Internal;
        else if (visibility is TypeAttributes.Public or TypeAttributes.NestedPublic)
            return Accessibility.Public;
        else 
            return Accessibility.None;
    }

    public static (Accessibility Getter, Accessibility Setter) GetAccessibility(PropertyInfo property)
    {
        var getter = GetAccessibility(property.GetMethod!);
        var setter = property.SetMethod is null ? Accessibility.None : GetAccessibility(property.SetMethod);
        return (getter, setter);
    }

    public static (Accessibility Adder, Accessibility Remover) GetAccessibility(EventInfo @event)
    {
        var adder = @event.AddMethod is null ? Accessibility.None : GetAccessibility(@event.AddMethod);
        var remover = @event.RemoveMethod is null ? Accessibility.None : GetAccessibility(@event.RemoveMethod);
        return (adder, remover);
    }

    public static string ToString(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Private => "private",
            Accessibility.PrivateProtected => "private protected",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedInternal => "protected internal",
            Accessibility.Internal => "internal",
            Accessibility.Public => "public",
            Accessibility.None or _ => ""
        };
    }
}