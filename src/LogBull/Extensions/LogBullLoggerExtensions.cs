using System;
using LogBull.Core;
using Microsoft.Extensions.Logging;

namespace LogBull.Extensions;

/// <summary>
/// Extension methods for adding LogBull to Microsoft.Extensions.Logging.
/// </summary>
public static class LogBullLoggerExtensions
{
    /// <summary>
    /// Adds LogBull logger provider to the logging builder.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="config">The LogBull configuration.</param>
    /// <returns>The logging builder for chaining.</returns>
    public static ILoggingBuilder AddLogBull(this ILoggingBuilder builder, Config config)
    {
        builder.AddProvider(new LogBullLoggerProvider(config));
        return builder;
    }

    /// <summary>
    /// Adds LogBull logger provider to the logging builder with configuration action.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The logging builder for chaining.</returns>
    public static ILoggingBuilder AddLogBull(this ILoggingBuilder builder, Action<Config.Builder> configure)
    {
        var configBuilder = Config.CreateBuilder();
        configure(configBuilder);
        var config = configBuilder.Build();

        return builder.AddLogBull(config);
    }
}

