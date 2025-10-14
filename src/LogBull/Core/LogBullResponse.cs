using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LogBull.Core;

/// <summary>
/// Response from LogBull server after sending logs.
/// </summary>
public class LogBullResponse
{
    /// <summary>
    /// Gets the number of logs accepted by the server.
    /// </summary>
    [JsonPropertyName("accepted")]
    public int Accepted { get; set; }

    /// <summary>
    /// Gets the number of logs rejected by the server.
    /// </summary>
    [JsonPropertyName("rejected")]
    public int Rejected { get; set; }

    /// <summary>
    /// Gets the response message.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Gets the list of rejected logs with error details.
    /// </summary>
    [JsonPropertyName("errors")]
    public List<RejectedLog>? Errors { get; set; }

    /// <summary>
    /// Represents a rejected log entry with error information.
    /// </summary>
    public class RejectedLog
    {
        /// <summary>
        /// Gets the index of the rejected log in the batch (0-based).
        /// </summary>
        [JsonPropertyName("index")]
        public int Index { get; set; }

        /// <summary>
        /// Gets the reason for rejection.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}

