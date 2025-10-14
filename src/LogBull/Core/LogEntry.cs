using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LogBull.Core;

/// <summary>
/// Represents a single log entry to be sent to LogBull.
/// </summary>
public record LogEntry
{
    /// <summary>
    /// Gets the log level.
    /// </summary>
    [JsonPropertyName("level")]
    public string Level { get; init; } = string.Empty;

    /// <summary>
    /// Gets the log message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the timestamp in RFC3339Nano format.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; init; } = string.Empty;

    /// <summary>
    /// Gets the custom fields associated with this log entry.
    /// </summary>
    [JsonPropertyName("fields")]
    public Dictionary<string, object> Fields { get; init; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry"/> class.
    /// </summary>
    public LogEntry()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry"/> class.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <param name="fields">The custom fields.</param>
    public LogEntry(string level, string message, string timestamp, Dictionary<string, object> fields)
    {
        Level = level;
        Message = message;
        Timestamp = timestamp;
        Fields = fields ?? new Dictionary<string, object>();
    }
}

