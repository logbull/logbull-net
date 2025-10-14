using System;

namespace LogBull.Internal.Timestamp;

/// <summary>
/// Generates unique, monotonically increasing timestamps with nanosecond precision.
/// </summary>
public class TimestampGenerator
{
    private readonly object _lock = new();
    private long _lastTimestampNanos;

    /// <summary>
    /// Generates a unique timestamp in RFC3339Nano format.
    /// </summary>
    /// <returns>A unique timestamp string.</returns>
    public string GenerateUniqueTimestamp()
    {
        lock (_lock)
        {
            // Get current time in nanoseconds
            // DateTime.UtcNow.Ticks is in 100-nanosecond intervals
            var ticks = DateTime.UtcNow.Ticks;
            var nanoseconds = ticks * 100; // Convert to nanoseconds

            // Ensure monotonicity
            if (nanoseconds <= _lastTimestampNanos)
            {
                nanoseconds = _lastTimestampNanos + 1;
            }

            _lastTimestampNanos = nanoseconds;
            return FormatTimestamp(nanoseconds);
        }
    }

    private string FormatTimestamp(long timestampNanos)
    {
        // Convert nanoseconds to DateTime
        // .NET ticks epoch is 0001-01-01, Unix epoch is 1970-01-01
        var unixEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        
        var totalTicks = timestampNanos / 100; // Convert to .NET ticks
        var ticks = unixEpochTicks + totalTicks;
        var dateTime = new DateTime(ticks, DateTimeKind.Utc);

        var nanos = timestampNanos % 1_000_000_000;
        
        // Format as RFC3339Nano: 2025-01-15T10:30:45.123456789Z
        return $"{dateTime:yyyy-MM-ddTHH:mm:ss}.{nanos:D9}Z";
    }
}

