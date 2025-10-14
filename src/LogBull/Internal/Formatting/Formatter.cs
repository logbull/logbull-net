using System;
using System.Collections.Generic;
using System.Text.Json;

namespace LogBull.Internal.Formatting;

/// <summary>
/// Formats log messages and fields for sending to LogBull.
/// </summary>
public class Formatter
{
    private const int MaxMessageLength = 10_000;

    /// <summary>
    /// Formats a log message by trimming whitespace and ensuring max length.
    /// </summary>
    /// <param name="message">The message to format.</param>
    /// <returns>The formatted message.</returns>
    public string FormatMessage(string? message)
    {
        if (message == null)
        {
            return string.Empty;
        }

        var trimmed = message.Trim();
        if (trimmed.Length > MaxMessageLength)
        {
            return trimmed.Substring(0, MaxMessageLength - 3) + "...";
        }

        return trimmed;
    }

    /// <summary>
    /// Ensures all fields are JSON-serializable, converting non-serializable values to strings.
    /// </summary>
    /// <param name="fields">The fields to ensure.</param>
    /// <returns>The ensured fields dictionary.</returns>
    public Dictionary<string, object> EnsureFields(Dictionary<string, object>? fields)
    {
        if (fields == null)
        {
            return new Dictionary<string, object>();
        }

        var formatted = new Dictionary<string, object>();
        foreach (var entry in fields)
        {
            var key = entry.Key?.Trim();
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            var value = entry.Value;
            if (IsJsonSerializable(value))
            {
                formatted[key] = value;
            }
            else
            {
                formatted[key] = ConvertToString(value);
            }
        }

        return formatted;
    }

    /// <summary>
    /// Merges two field dictionaries, with additional fields overriding base fields.
    /// </summary>
    /// <param name="baseFields">The base fields.</param>
    /// <param name="additionalFields">The additional fields to merge.</param>
    /// <returns>The merged fields dictionary.</returns>
    public Dictionary<string, object> MergeFields(
        Dictionary<string, object>? baseFields,
        Dictionary<string, object>? additionalFields)
    {
        var result = new Dictionary<string, object>(EnsureFields(baseFields));
        
        foreach (var entry in EnsureFields(additionalFields))
        {
            result[entry.Key] = entry.Value;
        }

        return result;
    }

    private bool IsJsonSerializable(object? value)
    {
        if (value == null)
        {
            return true;
        }

        try
        {
            JsonSerializer.Serialize(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string ConvertToString(object? value)
    {
        if (value == null)
        {
            return "null";
        }

        try
        {
            return JsonSerializer.Serialize(value);
        }
        catch
        {
            return value.ToString() ?? "null";
        }
    }
}

