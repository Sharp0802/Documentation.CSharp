using System.CommandLine;
using System.CommandLine.Parsing;
using Documentation.CSharp.Services;
using Microsoft.Build.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Documentation.CSharp;

public abstract class A<T1> 
{
    /// <summary>
    /// Foo of A
    /// </summary>
    public abstract void Foo<T>();
}

public class B : A<int>
{
    /// <inheritdoc cref="A{T1}.Foo{T}"/>
    public override void Foo<T>()
    {
        throw new NotImplementedException();
    }
}

internal static class Program
{
    private static Task Main(string[] args)
    {
        return Host
            .CreateDefaultBuilder(args)
            .ConfigureServices(services => services
                .AddHostedService<CommandLineService>())
            .Build()
            .RunAsync();
    }
}