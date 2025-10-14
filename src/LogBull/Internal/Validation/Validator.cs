using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LogBull.Internal.Validation;

/// <summary>
/// Validates configuration and log entry data.
/// </summary>
public class Validator
{
    private static readonly Regex UuidPattern = new(
        @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
        RegexOptions.Compiled);

    private static readonly Regex ApiKeyPattern = new(
        @"^[a-zA-Z0-9_\-.]{10,}$",
        RegexOptions.Compiled);

    private const int MaxMessageLength = 10_000;
    private const int MaxFieldsCount = 100;
    private const int MaxFieldKeyLength = 100;

    /// <summary>
    /// Validates the project ID format (must be a valid UUID).
    /// </summary>
    /// <param name="projectId">The project ID to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the project ID is invalid.</exception>
    public void ValidateProjectId(string projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
        {
            throw new ArgumentException("project ID cannot be empty");
        }

        var trimmed = projectId.Trim();
        if (!UuidPattern.IsMatch(trimmed))
        {
            throw new ArgumentException(
                $"invalid project ID format '{trimmed}'. Must be a valid UUID format: " +
                "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
        }
    }

    /// <summary>
    /// Validates the host URL format (must be a valid HTTP/HTTPS URL).
    /// </summary>
    /// <param name="host">The host URL to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the host URL is invalid.</exception>
    public void ValidateHostUrl(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new ArgumentException("host URL cannot be empty");
        }

        var trimmed = host.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"invalid host URL format: {trimmed}");
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException($"host URL must use http or https scheme, got: {uri.Scheme}");
        }

        if (string.IsNullOrEmpty(uri.Host))
        {
            throw new ArgumentException("host URL must have a host component");
        }
    }

    /// <summary>
    /// Validates the API key format.
    /// </summary>
    /// <param name="apiKey">The API key to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the API key is invalid.</exception>
    public void ValidateApiKey(string? apiKey)
    {
        if (apiKey == null)
        {
            return;
        }

        var trimmed = apiKey.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        if (trimmed.Length < 10)
        {
            throw new ArgumentException("API key must be at least 10 characters long");
        }

        if (!ApiKeyPattern.IsMatch(trimmed))
        {
            throw new ArgumentException(
                "invalid API key format. API key must contain only alphanumeric characters, " +
                "underscores, hyphens, and dots");
        }
    }

    /// <summary>
    /// Validates the log message.
    /// </summary>
    /// <param name="message">The log message to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the message is invalid.</exception>
    public void ValidateLogMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("log message cannot be empty");
        }

        if (message.Length > MaxMessageLength)
        {
            throw new ArgumentException(
                $"log message too long ({message.Length} chars). Maximum allowed: {MaxMessageLength}");
        }
    }

    /// <summary>
    /// Validates the log fields.
    /// </summary>
    /// <param name="fields">The log fields to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the fields are invalid.</exception>
    public void ValidateLogFields(Dictionary<string, object>? fields)
    {
        if (fields == null)
        {
            return;
        }

        if (fields.Count > MaxFieldsCount)
        {
            throw new ArgumentException(
                $"too many fields ({fields.Count}). Maximum allowed: {MaxFieldsCount}");
        }

        foreach (var key in fields.Keys)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("field key cannot be empty");
            }

            if (key.Length > MaxFieldKeyLength)
            {
                throw new ArgumentException(
                    $"field key too long ({key.Length} chars). Maximum: {MaxFieldKeyLength}");
            }
        }
    }
}

