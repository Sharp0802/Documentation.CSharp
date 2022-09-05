using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Documentation.CSharp.Compiler;

public class SourceGenerator
{
    private StringBuilder Builder { get; } = new();

    private static bool IsNullableAttribute(CustomAttributeData data)
    {
        return string.Equals(
            data.AttributeType.FullName,
            "System.Runtime.CompilerServices.NullableAttribute",
            StringComparison.Ordinal);
    }
    
    private static bool IsNullable(CustomAttributeData data)
    {
        // 1 for not-null; 2 for nullable
        return IsNullableAttribute(data) && (byte?) data.ConstructorArguments[0].Value == 2; 
    }

    private static string ToLiteral(string input)
    {
        using var writer = new StringWriter();
        using var provider = CodeDomProvider.CreateProvider("CSharp");
        provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, new CodeGeneratorOptions());
        return writer.ToString();
    }
    
    private static string LiteralToString(object? literal)
    {
        var builder = new StringBuilder();
        
        if (literal is bool boolValue) 
            builder.Append(boolValue ? "true" : "false");
        else if (literal is byte or sbyte or int or uint or nint or nuint or long or ulong or short or ushort)
            builder.Append(literal);
        else if (literal is decimal)
            builder.Append(literal).Append('D');
        else if (literal is double)
            builder.Append(literal);
        else if (literal is float)
            builder.Append(literal).Append("F");
        else if (literal is char charValue)
            builder.Append('\'').Append(charValue == '\'' ? "\\'" : charValue).Append('\'');
        else if (literal is string stringValue)
            builder.Append(ToLiteral(stringValue));

        return builder.ToString();
    }

    private static void GetDelegateInfo(
        Type type, 
        out Type[] genericArgs, 
        out ParameterInfo[] parameters, 
        out ParameterInfo @return)
    {
        var invoker = type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(method => method.Name.Equals("Invoke") && 
                              (method.Attributes & MethodAttributes.HideBySig) != 0 &&
                              (method.Attributes & MethodAttributes.NewSlot) != 0 &&
                              (method.Attributes & MethodAttributes.Virtual) != 0 &&
                              method.CallingConvention == CallingConventions.Standard &&
                              // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                              (method.MethodImplementationFlags & MethodImplAttributes.Runtime) != 0);

        genericArgs = type.GetGenericArguments();
        parameters = invoker.GetParameters();
        @return = invoker.ReturnParameter;
    }
    
    private static string TypeToString(Type type, bool nullable, bool raw)
    {
        string str;
        if (type.IsGenericParameter)
        {
            str = type.Name;
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            str = TypeToString(type.GetGenericArguments()[0], false, raw);
            nullable = true;
        }
        else if (type.IsByRef)
        {
            str = TypeToString(type.GetElementType()!, false, raw);
        }
        else if (type.IsArray)
        {
            var sb = new StringBuilder();
            while (type.IsArray)
            {
                sb.Append('[').Append(',', type.GetArrayRank() - 1).Append(']');
                type = type.GetElementType()!;
            }

            sb.Insert(0, TypeToString(type, false, raw));

            str = sb.ToString();
        }
        else if (type.IsPointer)
        {
            var i = 0;
            while (type.IsPointer)
            {
                type = type.GetElementType()!;
                i++;
            }

            var sb = new StringBuilder();
            sb.Append(TypeToString(type, false, raw)).Append('*', i);

            str = sb.ToString();
        }
        else
        {
            if (type == typeof(void)) str = "void";
            else if (type == typeof(bool)) str = "bool";
            else if (type == typeof(byte)) str = "byte";
            else if (type == typeof(sbyte)) str = "sbyte";
            else if (type == typeof(char)) str = "char";
            else if (type == typeof(decimal)) str = "decimal";
            else if (type == typeof(double)) str = "double";
            else if (type == typeof(float)) str = "float";
            else if (type == typeof(int)) str = "int";
            else if (type == typeof(uint)) str = "uint";
            else if (type == typeof(nint)) str = "nint";
            else if (type == typeof(nuint)) str = "nuint";
            else if (type == typeof(long)) str = "long";
            else if (type == typeof(ulong)) str = "ulong";
            else if (type == typeof(short)) str = "short";
            else if (type == typeof(ushort)) str = "ushort";
            else if (type == typeof(object)) str = "object";
            else if (type == typeof(string)) str = "string";
            else
            {
                str = raw ? type.Name : type.FullName!;
                var genericPos = str.LastIndexOf('`');
                if (genericPos != -1) 
                    str = str.Remove(genericPos);
            }
        }

        if (nullable) str += '?';

        return str;
    }

    private static string EscapeIdentifier(string id)
    {
        if (KeywordHelper.IsEscapingRequired(id))
            return '@' + id;
        return id;
    }

    private void WriteIdentifier(string id)
    {
        Builder.Append(EscapeIdentifier(id));
    }

    private void WriteTypeName(Type type, bool nullable)
    {
        Builder.Append(TypeToString(type, nullable, false));
    }

    private void WriteTypeNameRaw(Type type)
    {
        Builder.Append(TypeToString(type, false, true));
    }
    
    private void WriteAttribute(CustomAttributeData data, string? prefix, bool addBracket)
    {
        if (IsNullableAttribute(data)) return;
        
        if (addBracket) Builder.Append('[');
        
        if (prefix is not null)
        {
            Builder.Append(prefix);
            Builder.Append(": ");
        }

        var name = data.Constructor.DeclaringType!.FullName!;
        if (name.EndsWith("Attribute")) 
            name = name.Remove(name.Length - "Attribute".Length, "Attribute".Length);
        Builder.Append(name);

        if (data.ConstructorArguments.Count > 0 || data.NamedArguments.Count > 0)
        {
            Builder.Append('(');
        
            var first = true;
            foreach (var argument in data.ConstructorArguments)
            {
                if (!first) Builder.Append(", ");
                Builder.Append(argument.ToString());
                first = false;
            }

            foreach (var argument in data.NamedArguments)
            {
                if (!first) Builder.Append(", ");
                Builder.Append(argument.ToString());
                first = false;
            }

            Builder.Append(')');
        }
        
        if (addBracket) Builder.Append(']');
    }

    private void WriteAttributesInline(ref CustomAttributeData[] attributes, string? prefix)
    {
        attributes = attributes
            .Where(attribute => attribute.AttributeType != typeof(InAttribute) && 
                                attribute.AttributeType != typeof(OutAttribute) &&
                                attribute.AttributeType != typeof(IsReadOnlyAttribute) &&
                                !IsNullableAttribute(attribute))
            .ToArray();
        
        if (attributes.Length <= 0) return;
        
        Builder.Append('[');

        if (prefix is not null)
        {
            Builder.Append(prefix);
            Builder.Append(": ");
        }

        var first = true;
        foreach (var data in attributes)
        {
            if (!first) Builder.Append(", ");
            WriteAttribute(data, null, false);
            first = false;
        }

        Builder.Append(']');
    }
    
    private void WriteAccessibility(Accessibility accessibility)
    {
        Builder.Append(AccessibilityHelper.ToString(accessibility));
    }

    private void WriteModifier(Modifier modifier)
    {
        Builder.Append(ModifierHelper.ToString(modifier));
    }

    private void WriteParameter(ParameterInfo parameter)
    {
        var attributes = parameter
            .GetCustomAttributesData()
            .Where(data => parameter.HasDefaultValue ^ data.AttributeType == typeof(OptionalAttribute))
            .ToArray();
        var nullable = attributes.Any(IsNullable);
        WriteAttributesInline(ref attributes, null);
        if (attributes.Length > 0) WriteSpace();
        
        var direction = string.Empty;
        if (parameter.IsIn)
            direction = "in";
        else if (parameter.IsOut)
            direction = "out";
        else if (parameter.ParameterType.IsByRef)
            direction = "ref";
        Builder.Append(direction);
        
        if (direction.Length > 0) WriteSpace();
        WriteTypeName(parameter.ParameterType, nullable);
        WriteSpace();
        WriteIdentifier(parameter.Name!);

        if (parameter.HasDefaultValue)
        {
            var literal = LiteralToString(parameter.DefaultValue);
            if (!string.IsNullOrEmpty(literal))
            {
                WriteSpace();
                Builder.Append('=');
                WriteSpace();
                Builder.Append(literal);
            }
        }
    }
    
    private void WriteParameters(IEnumerable<ParameterInfo> parameters, bool addBracket)
    {
        if (addBracket) Builder.Append('(');
        
        var first = true;
        foreach (var param in parameters)
        {
            if (!first) Builder.Append(", ");
            WriteParameter(param);
            first = false;
        }
        
        if (addBracket) Builder.Append(')');
    }

    private void WriteGenericArguments(IReadOnlyCollection<Type> arguments)
    {
        if (arguments.Count <= 0) return;
        
        Builder.Append('<');

        var first = true;
        foreach (var argument in arguments)
        {
            if (!first) Builder.Append(", ");

            var attributes = argument.GenericParameterAttributes;
            if ((attributes & GenericParameterAttributes.Covariant) != 0)
                Builder.Append("out ");
            if ((attributes & GenericParameterAttributes.Contravariant) != 0)
                Builder.Append("in ");

            WriteTypeName(argument, false);
            first = false;
        }
        
        Builder.Append('>');
    }

    private void WriteGenericArgumentConstraints(IReadOnlyCollection<Type> arguments, bool onNextLine)
    {
        if (arguments.Count <= 0) return;

        var builder = new StringBuilder();
        foreach (var argument in arguments)
        {
            var constraints = argument.GetGenericParameterConstraints();
            var attributes = argument.GenericParameterAttributes;
            
            if (constraints.Length == 0 && (attributes & GenericParameterAttributes.SpecialConstraintMask) == 0)
                continue;

            var constraintsStrings = new List<string>();
            
            if ((attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                constraintsStrings.Add("class");
            if ((attributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                constraintsStrings.Add("new()");
            if ((attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                constraintsStrings.Add("notnull");
            
            foreach (var constraint in constraints)
            {
                if (constraint == typeof(ValueType))
                {
                    constraintsStrings.Add("struct");
                    constraintsStrings.Remove("new()");
                    constraintsStrings.Remove("notnull");
                }
                else
                {
                    constraintsStrings.Insert(0, TypeToString(constraint, false, false));
                }
            }

            builder.Append("where ");
            builder.Append(TypeToString(argument, false, false));
            builder.Append(" : ");
            builder.Append(string.Join(", ", constraintsStrings));
            if (onNextLine)
            {
                builder.Append("\n    ");
            }
            else
                builder.Append(' ');
        }
        
        if (onNextLine && builder.Length > 0)
        {
            WriteLinebreak();
            WriteTab();
        }

        var str = builder.ToString();
        if (str.EndsWith("\n    "))
            str = str.TrimEnd(' ', '\n');

        if (builder.Length > 0)
        {
            Builder.Append(str);
        }
    }
    
    private void WriteModifiers(IEnumerable<Modifier> modifiers)
    {
        foreach (var modifier in modifiers.SortModifiers().Except(new[] { Modifier.InitOnly }))
        {
            WriteSpace();
            WriteModifier(modifier);
        }
    }
    
    public void WriteMethodInfo(MethodInfo method)
    {
        foreach (var attribute in method.GetCustomAttributesData())
        {
            if (attribute.AttributeType.FullName?.Equals(
                    "System.Runtime.CompilerServices.NullableContextAttribute",
                    StringComparison.Ordinal) ?? false)
                continue;
            WriteAttribute(attribute, null, true);
            WriteLinebreak();
        }

        foreach (var attribute in method.ReturnParameter.GetCustomAttributesData())
        {
            WriteAttribute(attribute, "return", true);
            WriteLinebreak();
        }
        
        WriteAccessibility(AccessibilityHelper.GetAccessibility(method));
        WriteModifiers(ModifierHelper.GetModifiers(method));
        WriteSpace();
        WriteTypeName(method.ReturnType, method.ReturnParameter.GetCustomAttributesData().Any(IsNullable));
        WriteSpace();

        // private impl

        WriteIdentifier(method.Name);
        WriteGenericArguments(method.GetGenericArguments());
        WriteParameters(method.GetParameters(), true);
        WriteGenericArgumentConstraints(method.GetGenericArguments(), true);
    }

    public void WriteFieldInfo(FieldInfo field)
    {
        foreach (var attribute in field.CustomAttributes)
        {
            WriteAttribute(attribute, null, true);
            WriteLinebreak();
        }

        WriteAccessibility(AccessibilityHelper.GetAccessibility(field));
        WriteSpace();
        WriteModifiers(ModifierHelper.GetModifiers(field));
        WriteTypeName(field.FieldType, field.GetCustomAttributesData().Any(IsNullable));
        WriteSpace();
        WriteIdentifier(field.Name);
        Builder.Append(';');
    }

    public void WritePropertyInfo(PropertyInfo property)
    {
        foreach (var attribute in property.CustomAttributes)
        {
            WriteAttribute(attribute, null, true);
            WriteLinebreak();
        }

        var accessibility = AccessibilityHelper.GetAccessibility(property);
        WriteAccessibility(accessibility.Getter);
        WriteSpace();
        
        var modifiers = ModifierHelper.GetModifiers(property);
        modifiers.Setter = modifiers.Setter.Except(modifiers.Getter).ToArray();
        var propModifiers = modifiers.Getter.Except(new[] { Modifier.Readonly }).ToArray();
        if (propModifiers.Length > 0)
        {
            WriteModifiers(propModifiers);
            WriteSpace();   
        }
        
        WriteTypeName(property.PropertyType, property.GetCustomAttributesData().Any(IsNullable));
        WriteSpace();
        
        // private impl

        var indexers = property.GetIndexParameters();
        if (indexers.Length > 0)
        {
            Builder.Append("this[");
            WriteParameters(indexers, false);
            Builder.Append(']');
        }
        else
        {
            WriteIdentifier(property.Name);
        }

        WriteSpace();


        var multiline = false;

        var getterAttrs = property.GetMethod
            ?.GetCustomAttributesData()
            .ToArray() ?? Array.Empty<CustomAttributeData>();
        var getterReturnAttrs = property.GetMethod?.ReturnParameter
            .GetCustomAttributesData()
            .ToArray() ?? Array.Empty<CustomAttributeData>();
        var setterAttrs = property.SetMethod
            ?.GetCustomAttributesData()
            .ToArray() ?? Array.Empty<CustomAttributeData>();
        var setterReturnAttrs = property.SetMethod?.ReturnParameter
            .GetCustomAttributesData()
            .ToArray() ?? Array.Empty<CustomAttributeData>();
        if (getterAttrs.Length > 0 
            || getterReturnAttrs.Length > 0 
            || setterAttrs.Length > 0
            || setterReturnAttrs.Length > 0) 
            multiline = true;

        if (multiline)
            Builder.Append('\n');
        Builder.Append('{');
        if (multiline)
            WriteLinebreak();
        
        if (property.CanRead)
        {
            foreach (var attr in getterAttrs)
            {
                if (attr.AttributeType == typeof(IsReadOnlyAttribute)) continue;
                
                WriteTab();
                WriteAttribute(attr, null, true);
                WriteLinebreak();
            }

            foreach (var attr in getterReturnAttrs)
            {
                WriteTab();
                WriteAttribute(attr, "return", true);
                WriteLinebreak();
            }
            
            if (multiline)
                WriteTab();
            else
                WriteSpace();
            
            if (modifiers.Getter.Contains(Modifier.Readonly))
            {
                Builder.Append("readonly");
                WriteSpace();
            }
            Builder.Append("get;");

            if (multiline)
                WriteLinebreak();
        }

        if (property.CanWrite)
        {
            foreach (var attr in setterAttrs)
            {
                WriteTab();
                WriteAttribute(attr, null, true);
                WriteLinebreak();
            }

            foreach (var attr in setterReturnAttrs)
            {
                WriteTab();
                WriteAttribute(attr, "return", true);
                WriteLinebreak();
            }

            if (multiline)
                WriteTab();
            else
                WriteSpace();
            
            WriteModifiers(modifiers.Setter);
            if (modifiers.Setter.Contains(Modifier.InitOnly))
                Builder.Append("init;");
            else
                Builder.Append("set;");

            if (multiline)
                WriteLinebreak();
        }
        
        if (!multiline)
            WriteSpace();
        Builder.Append('}');
    }

    public void WriteEventInfo(EventInfo @event)
    {
        foreach (var attr in @event.CustomAttributes)
        {
            WriteAttribute(attr, null, true);
            WriteLinebreak();
        }

        var (accessibility, _) = AccessibilityHelper.GetAccessibility(@event);
        WriteAccessibility(accessibility);
        WriteSpace();
        
        var modifiers = ModifierHelper.GetModifiers(@event).Adder.ToArray();
        if (modifiers.Length > 0)
        {
            WriteModifiers(modifiers);
            WriteSpace();
        }

        Builder.Append("event");
        WriteSpace();
        WriteTypeName(@event.EventHandlerType!, false);
        WriteSpace();
        
        // private impl
        
        WriteIdentifier(@event.Name);
        WriteSpace();
        Builder.Append("{ add; remove; }");
    }
    
    private void WriteLinebreak()
    {
        Builder.Append('\n');
    }

    private void WriteSpace()
    {
        Builder.Append(' ');
    }

    private void WriteTab()
    {
        Builder.Append(' ', 4);
    }

    public override string ToString()
    {
        return Builder.ToString();
    }
}