using System.Collections.Generic;
using System.Linq;

namespace LogBull.Core;

/// <summary>
/// Represents a batch of log entries to be sent to LogBull.
/// </summary>
public class LogBatch
{
    /// <summary>
    /// Gets the logs in this batch.
    /// </summary>
    public List<LogEntry> Logs { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogBatch"/> class.
    /// </summary>
    /// <param name="logs">The log entries.</param>
    public LogBatch(List<LogEntry> logs)
    {
        Logs = logs ?? new List<LogEntry>();
    }

    /// <summary>
    /// Gets the number of logs in the batch.
    /// </summary>
    public int Size => Logs.Count;

    /// <summary>
    /// Gets a value indicating whether the batch is empty.
    /// </summary>
    public bool IsEmpty => !Logs.Any();
}

