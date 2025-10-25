using System;

namespace LogBull.Internal.Timestamp;

/// <summary>
/// Generates unique, monotonically increasing timestamps with nanosecond precision.
/// </summary>
public class TimestampGenerator
{
    private readonly object _lock = new();
    private long _lastTimestampNanos;

    // Unix epoch in .NET ticks (1970-01-01 00:00:00 UTC)
    private static readonly long UnixEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

    /// <summary>
    /// Generates a unique timestamp in RFC3339Nano format.
    /// </summary>
    /// <returns>A unique timestamp string.</returns>
    public string GenerateUniqueTimestamp()
    {
        lock (_lock)
        {
            // Get current time as Unix nanoseconds
            var currentTicks = DateTime.UtcNow.Ticks - UnixEpochTicks;
            var nanoseconds = currentTicks * 100; // Convert from 100-nanosecond ticks to nanoseconds

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
        // Split Unix nanoseconds into seconds and remaining nanoseconds
        var seconds = timestampNanos / 1_000_000_000;
        var nanos = timestampNanos % 1_000_000_000;

        // Convert Unix seconds to DateTime
        var dateTime = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;

        // Format as RFC3339Nano: 2025-01-15T10:30:45.123456789Z
        return $"{dateTime:yyyy-MM-ddTHH:mm:ss}.{nanos:D9}Z";
    }
}

