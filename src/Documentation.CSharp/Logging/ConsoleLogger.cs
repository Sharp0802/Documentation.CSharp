using Microsoft.Extensions.Logging;

namespace Documentation.CSharp.Logging;

public class ConsoleLogger : ILogger
{
    private string Name { get; }

    public ConsoleLogger(string name)
    {
        Name = name;
    }

    public ConsoleLogger(Type type) : this(type.FullName ?? "<unknown>")
    {
    }
    
    public void Log<TState>(
        LogLevel logLevel, 
        EventId eventId, 
        TState state, 
        Exception? exception, 
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var original = Console.ForegroundColor;
        Console.ForegroundColor = GetColor(logLevel);
        Console.WriteLine(formatter(state, exception));
        Console.ForegroundColor = original;
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;
    
    private static ConsoleColor GetColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Critical => ConsoleColor.Red,
            LogLevel.Error => ConsoleColor.DarkRed,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Information => ConsoleColor.White,
            LogLevel.Debug => ConsoleColor.Cyan,
            LogLevel.Trace => ConsoleColor.Magenta,
            _ => ConsoleColor.DarkGray
        };
    }

    private static string GetHeader(LogLevel level)
    {
        return level switch
        {
            LogLevel.Critical => "crit",
            LogLevel.Error => "fail",
            LogLevel.Warning => "warn",
            LogLevel.Information => "info",
            LogLevel.Debug => "dbug",
            LogLevel.Trace => "verb",
            _ => "none"
        };
    }
}