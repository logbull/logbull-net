using System;
using System.Collections.Generic;
using System.Linq;
using LogBull.Core;
using LogBull.Internal.Formatting;
using LogBull.Internal.Sender;
using LogBull.Internal.Timestamp;
using LogBull.Internal.Validation;

namespace LogBull;

/// <summary>
/// Standalone logger that sends logs to LogBull server.
/// </summary>
public class LogBullLogger : IDisposable
{
    private readonly Config _config;
    private readonly LogSender _sender;
    private readonly LogLevel _minLevel;
    private readonly Dictionary<string, object> _context;
    private readonly Validator _validator;
    private readonly Formatter _formatter;
    private readonly TimestampGenerator _timestampGenerator;
    private bool _disposed;

    private LogBullLogger(Config config, LogSender sender, Dictionary<string, object>? context)
    {
        _config = config;
        _sender = sender;
        _minLevel = config.LogLevel;
        _context = context ?? new Dictionary<string, object>();
        _validator = new Validator();
        _formatter = new Formatter();
        _timestampGenerator = new TimestampGenerator();
    }

    /// <summary>
    /// Creates a new LogBullLogger with the given configuration.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <returns>A new LogBullLogger instance.</returns>
    public static LogBullLogger Create(Config config)
    {
        var validator = new Validator();
        validator.ValidateProjectId(config.ProjectId);
        validator.ValidateHostUrl(config.Host);
        validator.ValidateApiKey(config.ApiKey);

        var sender = new LogSender(config);
        return new LogBullLogger(config, sender, null);
    }

    /// <summary>
    /// Creates a builder for constructing LogBullLogger instances.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public static Builder CreateBuilder()
    {
        return new Builder();
    }

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The log message.</param>
    public void Debug(string message)
    {
        Debug(message, null);
    }

    /// <summary>
    /// Logs a debug message with fields.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="fields">Additional fields.</param>
    public void Debug(string message, Dictionary<string, object>? fields)
    {
        Log(LogLevel.DEBUG, message, fields);
    }

    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="message">The log message.</param>
    public void Info(string message)
    {
        Info(message, null);
    }

    /// <summary>
    /// Logs an info message with fields.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="fields">Additional fields.</param>
    public void Info(string message, Dictionary<string, object>? fields)
    {
        Log(LogLevel.INFO, message, fields);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The log message.</param>
    public void Warning(string message)
    {
        Warning(message, null);
    }

    /// <summary>
    /// Logs a warning message with fields.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="fields">Additional fields.</param>
    public void Warning(string message, Dictionary<string, object>? fields)
    {
        Log(LogLevel.WARNING, message, fields);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The log message.</param>
    public void Error(string message)
    {
        Error(message, null);
    }

    /// <summary>
    /// Logs an error message with fields.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="fields">Additional fields.</param>
    public void Error(string message, Dictionary<string, object>? fields)
    {
        Log(LogLevel.ERROR, message, fields);
    }

    /// <summary>
    /// Logs a critical message.
    /// </summary>
    /// <param name="message">The log message.</param>
    public void Critical(string message)
    {
        Critical(message, null);
    }

    /// <summary>
    /// Logs a critical message with fields.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="fields">Additional fields.</param>
    public void Critical(string message, Dictionary<string, object>? fields)
    {
        Log(LogLevel.CRITICAL, message, fields);
    }

    /// <summary>
    /// Creates a new logger instance with additional context fields.
    /// The new logger shares the same sender instance.
    /// </summary>
    /// <param name="context">Additional context fields.</param>
    /// <returns>A new logger instance with merged context.</returns>
    public LogBullLogger WithContext(Dictionary<string, object> context)
    {
        var mergedContext = _formatter.MergeFields(_context, context);
        return new LogBullLogger(_config, _sender, mergedContext);
    }

    /// <summary>
    /// Immediately sends all queued logs to LogBull server.
    /// </summary>
    public void Flush()
    {
        _sender.Flush();
    }

    /// <summary>
    /// Stops the logger and sends all remaining logs.
    /// </summary>
    public void Shutdown()
    {
        _sender.Shutdown();
    }

    private void Log(LogLevel level, string message, Dictionary<string, object>? fields)
    {
        try
        {
            // Check log level
            if (level.GetPriority() < _minLevel.GetPriority())
            {
                return;
            }

            // Validate inputs
            _validator.ValidateLogMessage(message);
            _validator.ValidateLogFields(fields);

            // Merge context and fields
            var mergedFields = _formatter.MergeFields(_context, fields);

            // Format message and fields
            var formattedMessage = _formatter.FormatMessage(message);
            var ensuredFields = _formatter.EnsureFields(mergedFields);

            // Generate unique timestamp
            var timestamp = _timestampGenerator.GenerateUniqueTimestamp();

            // Create log entry
            var entry = new LogEntry(
                level.ToString(),
                formattedMessage,
                timestamp,
                ensuredFields);

            // Print to console
            PrintToConsole(entry);

            // Add to send queue
            _sender.AddLog(entry);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"LogBull: invalid log message: {ex.Message}");
        }
    }

    private void PrintToConsole(LogEntry entry)
    {
        var output = $"[{entry.Timestamp}] [{entry.Level}] {entry.Message}";

        if (entry.Fields.Any())
        {
            var fieldsParts = entry.Fields.Select(f => $"{f.Key}={f.Value}");
            output += $" ({string.Join(", ", fieldsParts)})";
        }

        if (entry.Level == "ERROR" || entry.Level == "CRITICAL")
        {
            Console.Error.WriteLine(output);
        }
        else
        {
            Console.WriteLine(output);
        }
    }

    /// <summary>
    /// Disposes the logger and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Shutdown();
    }

    /// <summary>
    /// Builder for LogBullLogger.
    /// </summary>
    public class Builder
    {
        private string? _projectId;
        private string? _host;
        private string? _apiKey;
        private LogLevel _logLevel = LogLevel.INFO;

        /// <summary>
        /// Sets the project ID.
        /// </summary>
        /// <param name="projectId">The project ID.</param>
        /// <returns>This builder instance.</returns>
        public Builder WithProjectId(string projectId)
        {
            _projectId = projectId;
            return this;
        }

        /// <summary>
        /// Sets the host URL.
        /// </summary>
        /// <param name="host">The host URL.</param>
        /// <returns>This builder instance.</returns>
        public Builder WithHost(string host)
        {
            _host = host;
            return this;
        }

        /// <summary>
        /// Sets the API key.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <returns>This builder instance.</returns>
        public Builder WithApiKey(string? apiKey)
        {
            _apiKey = apiKey;
            return this;
        }

        /// <summary>
        /// Sets the log level.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <returns>This builder instance.</returns>
        public Builder WithLogLevel(LogLevel logLevel)
        {
            _logLevel = logLevel;
            return this;
        }

        /// <summary>
        /// Builds the LogBullLogger instance.
        /// </summary>
        /// <returns>A new LogBullLogger instance.</returns>
        public LogBullLogger Build()
        {
            var config = Config.CreateBuilder()
                .WithProjectId(_projectId ?? throw new InvalidOperationException("ProjectId is required"))
                .WithHost(_host ?? throw new InvalidOperationException("Host is required"))
                .WithApiKey(_apiKey)
                .WithLogLevel(_logLevel)
                .Build();

            return Create(config);
        }
    }
}

