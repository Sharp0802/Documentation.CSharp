using Documentation.CSharp.Commands;
using Documentation.CSharp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Documentation.CSharp;

internal static class Program
{
    private static Task Main(string[] args)
    {
        return Host
            .CreateDefaultBuilder(args)
            .ConfigureServices(services => services
                .AddSingleton<CommandRegistrar>()
                .AddHostedService<CommandLineService>())
            .Build()
            .RunAsync();
    }
}