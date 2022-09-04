using System.Reflection;
using System.Runtime.CompilerServices;

namespace Documentation.CSharp.Compiler;

public static class ModifierHelper
{
    public static bool IsOverriden(MethodInfo method)
    {
        return method.ReflectedType != method.GetBaseDefinition().ReflectedType;
    }

    public static bool IsCompilerGenerated(MemberInfo member)
    {
        return member.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Any();
    }

    public static bool IsNewSlotRequired(MethodBase method)
    {
        if ((method.Attributes & MethodAttributes.NewSlot) != 0)
            return true;
        return false;
    }

    public static IEnumerable<Modifier> GetModifiers(MethodBase method)
    {
        var flags = method.Attributes;

        if ((flags & MethodAttributes.Static) != 0)
            yield return Modifier.Static;
        if ((flags & MethodAttributes.Abstract) != 0)
            yield return Modifier.Abstract;
        if ((flags & MethodAttributes.Virtual) != 0)
            yield return Modifier.Virtual;
        if ((flags & MethodAttributes.Final) != 0)
            yield return Modifier.Sealed;
        if (method.CustomAttributes.Any(data => data.AttributeType == typeof(IsReadOnlyAttribute)))
            yield return Modifier.Readonly;
        if ((flags & MethodAttributes.PinvokeImpl) != 0)
            yield return Modifier.Extern;
        if (IsNewSlotRequired(method))
            yield return Modifier.New;

        if (method is MethodInfo info)
        {
            if (info.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit)))
                yield return Modifier.InitOnly;
            if (IsOverriden(info))
                yield return Modifier.Override;
        }
    }

    public static IEnumerable<Modifier> GetModifiers(FieldInfo field)
    {
        if (field.IsLiteral)
            yield return Modifier.Const;
        if (field.IsStatic)
            yield return Modifier.Static;
        if (field.IsInitOnly)
            yield return Modifier.Readonly;
        if (field.GetRequiredCustomModifiers().Contains(typeof(IsVolatile)))
            yield return Modifier.Volatile;
    }

    public static IEnumerable<Modifier> GetModifiers(Type type)
    {
        if (type.IsAbstract)
            yield return Modifier.Abstract;
        if (type.IsSealed)
            yield return Modifier.Sealed;
        if (type.IsAbstract && type.IsSealed)
            yield return Modifier.Static;
        if (type.CustomAttributes.Any(data => data.AttributeType == typeof(IsReadOnlyAttribute)))
            yield return Modifier.Readonly;
    }

    public static (IEnumerable<Modifier> Getter, IEnumerable<Modifier> Setter) GetModifiers(PropertyInfo property)
    {
        var getter = GetModifiers(property.GetMethod!);
        if (IsCompilerGenerated(property.GetMethod!))
            getter = getter.Except(new[] { Modifier.Readonly });

        var setter = property.SetMethod is null 
            ? Enumerable.Empty<Modifier>() 
            : GetModifiers(property.SetMethod);
        
        return (getter, setter);
    }

    public static (IEnumerable<Modifier> Adder, IEnumerable<Modifier> Remover) GetModifiers(EventInfo @event)
    {
        var adder = GetModifiers(@event.AddMethod!);
        var remover = GetModifiers(@event.RemoveMethod!);
        return (adder, remover);
    }

    public static string ToString(Modifier modifier)
    {
        return modifier switch
        {
            Modifier.New => "new",
            Modifier.Const => "const",
            Modifier.Static => "static",
            Modifier.Abstract => "abstract",
            Modifier.Virtual => "virtual",
            Modifier.Sealed => "sealed",
            Modifier.Readonly => "readonly",
            Modifier.InitOnly => "init",
            Modifier.Override => "override",
            Modifier.Extern => "extern",
            Modifier.Volatile => "volatile",
            _ => ""
        };
    }

    private class ModifierComparer : IComparer<Modifier>
    {
        public int Compare(Modifier x, Modifier y) => (int)x - (int)y;
    }

    public static IEnumerable<Modifier> SortModifiers(this IEnumerable<Modifier> modifiers)
    {
        return modifiers.OrderBy(mod => mod, new ModifierComparer());
    }
}