namespace LogBull.Core;

/// <summary>
/// Log severity levels supported by LogBull.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Detailed information for debugging (priority: 10).
    /// </summary>
    DEBUG = 10,

    /// <summary>
    /// General information messages (priority: 20).
    /// </summary>
    INFO = 20,

    /// <summary>
    /// Warning messages (priority: 30).
    /// </summary>
    WARNING = 30,

    /// <summary>
    /// Error messages (priority: 40).
    /// </summary>
    ERROR = 40,

    /// <summary>
    /// Critical error messages (priority: 50).
    /// </summary>
    CRITICAL = 50
}

/// <summary>
/// Extension methods for LogLevel.
/// </summary>
public static class LogLevelExtensions
{
    /// <summary>
    /// Gets the priority value of the log level.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <returns>The priority value.</returns>
    public static int GetPriority(this LogLevel level)
    {
        return (int)level;
    }
}

