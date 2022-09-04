using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Documentation.CSharp;

public sealed class ParseContext : IDisposable
{
    public ParseContext(IEnumerable<AssemblyName> assemblies)
    {
        var files = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
        
        var resolver = new PathAssemblyResolver(files);
        
        MetadataLoadContext = new MetadataLoadContext(resolver);
        foreach (var assembly in assemblies)
            MetadataLoadContext.LoadFromAssemblyName(assembly);
        Assemblies = MetadataLoadContext.GetAssemblies().ToArray();
    }


    private readonly Regex _memberParser = new(
        @"(?<name>[\w#]+)(?:``(?<n_generic>\d+))?(?:[(](?<args>(?:[^),]+,?)*)[)])?",
        RegexOptions.Compiled);

    private MetadataLoadContext MetadataLoadContext { get; }
    
    private Assembly[] Assemblies { get; }

    private ConcurrentDictionary<string, Type?> TypeQueryCache { get; } = new();
    

    private MethodBase? GetMethod(Type type, string name, int genericParameterCount, string[] parameterTypes)
    {
        var methods = name != "#ctor"
            ? type
                .GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic)
                .Where(method => method.Name == name)
                .Cast<MethodBase>()
                .ToArray()
            : type
                .GetConstructors(
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

        return methods
            .Where(method =>
            {
                var genericArguments = method.GetGenericArguments();
                if (genericArguments.Length != genericParameterCount) return false;

                var parameters = method.GetParameters();
                if (parameters.Length != parameterTypes.Length) return false;

                var equals = true;
                for (var i = 0; i < parameters.Length; ++i)
                {
                    var parameter = parameters[i];
                    var expectedT = parameterTypes[i];

                    var refRequired = expectedT.EndsWith('@');
                    if (refRequired) expectedT = expectedT[..^2];

                    var paramT = parameter.ParameterType;
                    var isRef = paramT.IsByRef;
                    if (isRef) paramT = paramT.GetElementType();

                    equals &= isRef == refRequired;

                    if (!equals) break;

                    if (expectedT.StartsWith("``"))
                    {
                        if (!int.TryParse(expectedT, out var genericArgumentIdx))
                            genericArgumentIdx = -1;

                        equals &= paramT == genericArguments[genericArgumentIdx];
                    }
                    else
                    {
                        equals &= paramT == GetType(expectedT);
                    }

                    if (!equals) break;
                }

                return equals;
            })
            .FirstOrDefault();
    }

    private MemberInfo ParseKey(string key)
    {
        var matches = _memberParser.Matches(key).ToArray();

        if (key.StartsWith("T:"))
        {
            var name = string.Join('.', matches[1..].Select(match => match.Groups["name"].Value));
            var type = GetType(name);
            if (type is null)
                throw new TypeAccessException($"Failed to find a type by name: {name}");

            return type;
        }
        else if (key.StartsWith("M:"))
        {
            var className = string.Join('.', matches[1..^2].Select(match => match.Groups["name"].Value));
            var methodName = matches[^1];

            var name = methodName.Groups["name"].Value;
            var nGenericStr = methodName.Groups["n_generic"].Value;
            var argTs = methodName.Groups["args"].Value.Split(',');

            var type = GetType(className);
            if (type is not null)
            {
                if (!int.TryParse(nGenericStr, out var nGeneric))
                    nGeneric = -1;
                var method = GetMethod(type, name, nGeneric, argTs);
                if (method is null)
                    throw new MissingMethodException($"Failed to find a method in type by name", name);

                return method;
            }
            else
            {
                throw new TypeAccessException($"Failed to find a type by name: {className}");
            }
        }
        else if (key.StartsWith("P:"))
        {
            var className = string.Join('.', matches[1..^2].Select(match => match.Groups["name"].Value));
            var propertyName = matches[^1].Groups["name"].Value;

            var type = GetType(className);
            if (type is not null)
            {
                var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (property is null)
                    throw new MissingMemberException("Failed to find a property in type by name", propertyName);

                return property;
            }
            else
            {
                throw new TypeAccessException($"Failed to find a type by name: {className}");
            }
        }
        else if (key.StartsWith("F:"))
        {
            var className = string.Join('.', matches[1..^2].Select(match => match.Groups["name"].Value));
            var fieldName = matches[^1].Groups["name"].Value;

            var type = GetType(className);
            if (type is not null)
            {
                var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (field is null)
                    throw new MissingFieldException("Failed to find a field in type by name", fieldName);

                return field;
            }
            else
            {
                throw new TypeAccessException($"Failed to find a type by name: {className}");
            }
        }
        else if (key.StartsWith("E:"))
        {
            var className = string.Join('.', matches[1..^2].Select(match => match.Groups["name"].Value));
            var eventName = matches[^1].Groups["name"].Value;

            var type = GetType(className);
            if (type is not null)
            {
                var @event = type.GetEvent(eventName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (@event is null)
                    throw new MissingMemberException("Failed to find a event in type by name", eventName);

                return @event;
            }
            else
            {
                throw new TypeAccessException($"Failed to find a type by name: {className}");
            }
        }
        else
        {
            throw new NotSupportedException(
                "Invalid documentation member detected. " +
                "the documentation member must be one of Type(class, struct), Method, Property, Field, Event.");
        }
    }


    public Type? GetType(string name)
    {
        return TypeQueryCache
            .GetOrAdd(name, n => Assemblies
                .AsParallel()
                .Select(asm => asm.GetType(n))
                .FirstOrDefault(type => type is not null));
    }

    public Dictionary<MemberInfo, XElement> Parse(IEnumerable<XDocument> documents)
    {
        return documents
            .AsParallel()
            .Select(doc => doc.Root)
            .Where(root => root is not null)
            .Elements("members")
            .SelectMany(members => members.Elements("member"))
            .Select(member => (Key: member.Attribute("name"), Value: member))
            .Where(e => e.Key is not null)
            .ToDictionary(e => ParseKey(e.Key!.Value), e => e.Value);
    }

    public void Dispose()
    {
        MetadataLoadContext.Dispose();
    }
}