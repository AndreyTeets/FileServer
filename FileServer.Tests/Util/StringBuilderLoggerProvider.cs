﻿using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace FileServer.Tests.Util;

public class StringBuilderLoggerProvider : ILoggerProvider
{
    private static readonly SemaphoreSlim s_sbSemaphoreSlim = new(1, 1);
    private readonly StringBuilder _sb;

    public StringBuilderLoggerProvider(StringBuilder sb)
    {
        _sb = sb;
    }

    public ILogger CreateLogger(string name)
    {
        return new StringBuilderLogger(name, _sb);
    }

    public void Dispose()
    {
    }

    public class StringBuilderLogger : ILogger
    {
        private readonly string _name;
        private readonly StringBuilder _sb;

        public StringBuilderLogger(string name, StringBuilder sb)
        {
            _name = name;
            _sb = sb;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter is null)
                throw new ArgumentNullException(nameof(formatter));

            string message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
                return;

            message = $"{GetDisplayLevel(logLevel)}: {_name}{Environment.NewLine}{Indent(message)}";
            if (exception is not null)
                message += $"{Environment.NewLine}{Indent(exception.ToString())}";

            s_sbSemaphoreSlim.Wait();
            try
            {
                _sb.Append(message).AppendLine();
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
                    _ => "",
                };
            }

            static string Indent(string input)
            {
                return Regex.Replace(input, @"^", "      ", RegexOptions.Multiline);
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return new NoopDisposable();
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
