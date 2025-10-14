using System.Collections.Generic;

namespace LogBull.Core;

/// <summary>
/// Response from LogBull server after sending logs.
/// </summary>
public class LogBullResponse
{
    /// <summary>
    /// Gets the number of logs accepted by the server.
    /// </summary>
    public int Accepted { get; set; }

    /// <summary>
    /// Gets the number of logs rejected by the server.
    /// </summary>
    public int Rejected { get; set; }

    /// <summary>
    /// Gets the response message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets the list of rejected logs with error details.
    /// </summary>
    public List<RejectedLog>? Errors { get; set; }

    /// <summary>
    /// Represents a rejected log entry with error information.
    /// </summary>
    public class RejectedLog
    {
        /// <summary>
        /// Gets the index of the rejected log in the batch (0-based).
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets the reason for rejection.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}

