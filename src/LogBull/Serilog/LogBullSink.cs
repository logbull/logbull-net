using System;
using System.Collections.Generic;
using System.Linq;
using LogBull.Core;
using LogBull.Internal.Formatting;
using LogBull.Internal.Sender;
using LogBull.Internal.Timestamp;
using Serilog.Core;
using Serilog.Events;

namespace LogBull.Serilog;

/// <summary>
/// Serilog sink that sends logs to LogBull server.
/// </summary>
public class LogBullSink : ILogEventSink, IDisposable
{
    private readonly Config _config;
    private readonly LogSender _sender;
    private readonly Formatter _formatter;
    private readonly TimestampGenerator _timestampGenerator;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogBullSink"/> class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    public LogBullSink(Config config)
    {
        _config = config;
        _sender = new LogSender(config);
        _formatter = new Formatter();
        _timestampGenerator = new TimestampGenerator();
    }

    /// <summary>
    /// Emits a log event to LogBull.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    public void Emit(LogEvent logEvent)
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            var level = ConvertSerilogLevel(logEvent.Level);

            // Check if level is enabled
            if (level.GetPriority() < _config.LogLevel.GetPriority())
            {
                return;
            }

            // Render message with template
            var message = logEvent.RenderMessage();

            // Extract properties
            var fields = ExtractProperties(logEvent);

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

    /// <summary>
    /// Flushes all pending logs.
    /// </summary>
    public void Flush()
    {
        _sender.Flush();
    }

    /// <summary>
    /// Disposes the sink and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _sender.Dispose();
    }

    private LogLevel ConvertSerilogLevel(LogEventLevel level)
    {
        return level switch
        {
            LogEventLevel.Verbose => LogLevel.DEBUG,
            LogEventLevel.Debug => LogLevel.DEBUG,
            LogEventLevel.Information => LogLevel.INFO,
            LogEventLevel.Warning => LogLevel.WARNING,
            LogEventLevel.Error => LogLevel.ERROR,
            LogEventLevel.Fatal => LogLevel.CRITICAL,
            _ => LogLevel.INFO
        };
    }

    private Dictionary<string, object> ExtractProperties(LogEvent logEvent)
    {
        var fields = new Dictionary<string, object>();

        foreach (var property in logEvent.Properties)
        {
            var value = ExtractPropertyValue(property.Value);
            fields[property.Key] = value;
        }

        // Add exception if present
        if (logEvent.Exception != null)
        {
            fields["exception"] = logEvent.Exception.ToString();
            fields["exception_type"] = logEvent.Exception.GetType().Name;
            fields["exception_message"] = logEvent.Exception.Message;
        }

        return fields;
    }

    private object ExtractPropertyValue(LogEventPropertyValue value)
    {
        return value switch
        {
            ScalarValue scalarValue => scalarValue.Value ?? "null",
            SequenceValue sequenceValue => sequenceValue.Elements.Select(ExtractPropertyValue).ToList(),
            StructureValue structureValue => structureValue.Properties.ToDictionary(
                p => p.Name,
                p => ExtractPropertyValue(p.Value)),
            DictionaryValue dictionaryValue => dictionaryValue.Elements.ToDictionary(
                kvp => ExtractPropertyValue(kvp.Key).ToString() ?? string.Empty,
                kvp => ExtractPropertyValue(kvp.Value)),
            _ => value.ToString()
        };
    }
}

