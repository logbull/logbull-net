using System;
using System.Collections.Generic;
using LogBull.Core;
using LogBull.Internal.Formatting;
using LogBull.Internal.Sender;
using LogBull.Internal.Timestamp;
using Microsoft.Extensions.Logging;

namespace LogBull.Extensions;

/// <summary>
/// Logger implementation for Microsoft.Extensions.Logging.
/// </summary>
public class LogBullMelLogger : ILogger
{
    private readonly string _categoryName;
    private readonly Config _config;
    private readonly LogSender _sender;
    private readonly Formatter _formatter;
    private readonly TimestampGenerator _timestampGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogBullMelLogger"/> class.
    /// </summary>
    /// <param name="categoryName">The category name.</param>
    /// <param name="config">The configuration.</param>
    /// <param name="sender">The log sender.</param>
    public LogBullMelLogger(string categoryName, Config config, LogSender sender)
    {
        _categoryName = categoryName;
        _config = config;
        _sender = sender;
        _formatter = new Formatter();
        _timestampGenerator = new TimestampGenerator();
    }

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="state">The state.</param>
    /// <returns>A disposable scope.</returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        // Scope support can be added if needed
        return null;
    }

    /// <summary>
    /// Checks if the given log level is enabled.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <returns>True if enabled; otherwise, false.</returns>
    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        var mappedLevel = ConvertLogLevel(logLevel);
        return mappedLevel.GetPriority() >= _config.LogLevel.GetPriority();
    }

    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="logLevel">The log level.</param>
    /// <param name="eventId">The event ID.</param>
    /// <param name="state">The state.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="formatter">The formatter function.</param>
    public void Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        try
        {
            var level = ConvertLogLevel(logLevel);
            var message = formatter(state, exception);

            if (exception != null)
            {
                message = $"{message} {exception}";
            }

            // Extract fields from state
            var fields = ExtractFields(state, eventId);

            // Add category name
            fields["category"] = _categoryName;

            // Format message and fields
            var formattedMessage = _formatter.FormatMessage(message);
            var ensuredFields = _formatter.EnsureFields(fields);

            // Generate timestamp
            var timestamp = _timestampGenerator.GenerateUniqueTimestamp();

            // Create log entry
            var entry = new LogEntry(
                level.ToString(),
                formattedMessage,
                timestamp,
                ensuredFields);

            _sender.AddLog(entry);
        }
        catch
        {
            // Never throw from log method
        }
    }

    private LogLevel ConvertLogLevel(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        return logLevel switch
        {
            Microsoft.Extensions.Logging.LogLevel.Trace => LogLevel.DEBUG,
            Microsoft.Extensions.Logging.LogLevel.Debug => LogLevel.DEBUG,
            Microsoft.Extensions.Logging.LogLevel.Information => LogLevel.INFO,
            Microsoft.Extensions.Logging.LogLevel.Warning => LogLevel.WARNING,
            Microsoft.Extensions.Logging.LogLevel.Error => LogLevel.ERROR,
            Microsoft.Extensions.Logging.LogLevel.Critical => LogLevel.CRITICAL,
            _ => LogLevel.INFO
        };
    }

    private Dictionary<string, object> ExtractFields<TState>(TState state, EventId eventId)
    {
        var fields = new Dictionary<string, object>();

        if (eventId.Id != 0)
        {
            fields["event_id"] = eventId.Id;
        }

        if (!string.IsNullOrEmpty(eventId.Name))
        {
            fields["event_name"] = eventId.Name;
        }

        // Try to extract structured logging state
        if (state is IEnumerable<KeyValuePair<string, object>> keyValuePairs)
        {
            foreach (var kvp in keyValuePairs)
            {
                // Skip the original format template
                if (kvp.Key == "{OriginalFormat}")
                {
                    continue;
                }

                fields[kvp.Key] = kvp.Value;
            }
        }

        return fields;
    }
}

