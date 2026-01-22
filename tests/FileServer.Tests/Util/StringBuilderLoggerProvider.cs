using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace FileServer.Tests.Util;

internal sealed class StringBuilderLoggerProvider(
    StringBuilder sb)
    : ILoggerProvider
{
    private static readonly SemaphoreSlim s_sbSemaphoreSlim = new(1, 1);
    private readonly StringBuilder _sb = sb;

    public ILogger CreateLogger(string name) =>
        new StringBuilderLogger(name, _sb);

    public void Dispose() { }

    internal sealed class StringBuilderLogger(
        string name,
        StringBuilder sb)
        : ILogger
    {
        private readonly string _name = name;
        private readonly StringBuilder _sb = sb;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            ArgumentNullException.ThrowIfNull(formatter);

            string message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
                return;

            message = $"{GetDisplayLevel(logLevel)}: {_name}{Environment.NewLine}{Indent(message)}";
            if (exception is not null)
                message += $"{Environment.NewLine}{Indent(exception.ToString())}";

            s_sbSemaphoreSlim.Wait();
            try
            {
                _sb.AppendLine(message);
            }
            finally
            {
                s_sbSemaphoreSlim.Release();
            }

            static string GetDisplayLevel(LogLevel logLevel)
            {
                return logLevel switch
                {
                    LogLevel.Trace => "trce",
                    LogLevel.Debug => "dbug",
                    LogLevel.Information => "info",
                    LogLevel.Warning => "warn",
                    LogLevel.Error => "fail",
                    LogLevel.Critical => "crit",
                    LogLevel.None or _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
                };
            }

            static string Indent(string input) =>
                Regex.Replace(input, "^", "      ", RegexOptions.Multiline);
        }

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel != LogLevel.None;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
            new NoopDisposable();

        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
