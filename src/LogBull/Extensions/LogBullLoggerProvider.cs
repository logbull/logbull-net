using System;
using System.Collections.Concurrent;
using LogBull.Core;
using LogBull.Internal.Sender;
using Microsoft.Extensions.Logging;

namespace LogBull.Extensions;

/// <summary>
/// Logger provider for Microsoft.Extensions.Logging integration.
/// </summary>
[ProviderAlias("LogBull")]
public class LogBullLoggerProvider : ILoggerProvider
{
    private readonly Config _config;
    private readonly LogSender _sender;
    private readonly ConcurrentDictionary<string, LogBullMelLogger> _loggers;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogBullLoggerProvider"/> class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    public LogBullLoggerProvider(Config config)
    {
        _config = config;
        _sender = new LogSender(config);
        _loggers = new ConcurrentDictionary<string, LogBullMelLogger>();
    }

    /// <summary>
    /// Creates a new logger instance for the given category name.
    /// </summary>
    /// <param name="categoryName">The category name.</param>
    /// <returns>A new logger instance.</returns>
    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new LogBullMelLogger(name, _config, _sender));
    }

    /// <summary>
    /// Disposes the provider and releases resources.
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
}

