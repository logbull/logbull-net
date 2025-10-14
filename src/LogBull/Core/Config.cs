using System;
using LogBull.Internal.Validation;

namespace LogBull.Core;

/// <summary>
/// Configuration for LogBull client.
/// </summary>
public class Config
{
    /// <summary>
    /// Gets the project ID.
    /// </summary>
    public string ProjectId { get; }

    /// <summary>
    /// Gets the LogBull server host URL.
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Gets the API key for authentication (optional).
    /// </summary>
    public string? ApiKey { get; }

    /// <summary>
    /// Gets the minimum log level to process.
    /// </summary>
    public LogLevel LogLevel { get; }

    private Config(Builder builder)
    {
        ProjectId = builder.ProjectId ?? throw new ArgumentNullException(nameof(builder.ProjectId), "ProjectId cannot be null");
        Host = builder.Host ?? throw new ArgumentNullException(nameof(builder.Host), "Host cannot be null");
        ApiKey = builder.ApiKey;
        LogLevel = builder.LogLevel;
    }

    /// <summary>
    /// Creates a new builder for Config.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public static Builder CreateBuilder()
    {
        return new Builder();
    }

    /// <summary>
    /// Builder for creating Config instances.
    /// </summary>
    public class Builder
    {
        /// <summary>
        /// Gets or sets the project ID.
        /// </summary>
        public string? ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the host URL.
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.INFO;

        /// <summary>
        /// Sets the project ID.
        /// </summary>
        /// <param name="projectId">The project ID.</param>
        /// <returns>This builder instance.</returns>
        public Builder WithProjectId(string projectId)
        {
            ProjectId = projectId;
            return this;
        }

        /// <summary>
        /// Sets the host URL.
        /// </summary>
        /// <param name="host">The host URL.</param>
        /// <returns>This builder instance.</returns>
        public Builder WithHost(string host)
        {
            Host = host;
            return this;
        }

        /// <summary>
        /// Sets the API key.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <returns>This builder instance.</returns>
        public Builder WithApiKey(string? apiKey)
        {
            ApiKey = apiKey;
            return this;
        }

        /// <summary>
        /// Sets the log level.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <returns>This builder instance.</returns>
        public Builder WithLogLevel(LogLevel logLevel)
        {
            LogLevel = logLevel;
            return this;
        }

        /// <summary>
        /// Builds and validates the Config instance.
        /// </summary>
        /// <returns>A new Config instance.</returns>
        public Config Build()
        {
            // Validate configuration
            var validator = new Validator();
            validator.ValidateProjectId(ProjectId ?? string.Empty);
            validator.ValidateHostUrl(Host ?? string.Empty);
            validator.ValidateApiKey(ApiKey);

            return new Config(this);
        }
    }
}

