using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LogBull.Core;

namespace LogBull.Internal.Sender;

/// <summary>
/// Handles asynchronous sending of log batches to LogBull server.
/// </summary>
public class LogSender : IDisposable
{
    private const int BatchSize = 1_000;
    private const int BatchIntervalMs = 1_000;
    private const int QueueCapacity = 10_000;
    private const int MinWorkers = 1;
    private const int MaxWorkers = 10;
    private const int HttpTimeoutMs = 30_000;
    private const int MaxRetries = 3;

    private readonly Config _config;
    private readonly HttpClient _httpClient;
    private readonly BlockingCollection<LogEntry> _logQueue;
    private readonly SemaphoreSlim _workerSemaphore;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _batchProcessorTask;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogSender"/> class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    public LogSender(Config config)
    {
        _config = config;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(HttpTimeoutMs)
        };
        _logQueue = new BlockingCollection<LogEntry>(QueueCapacity);
        _workerSemaphore = new SemaphoreSlim(MinWorkers, MaxWorkers);
        _cancellationTokenSource = new CancellationTokenSource();

        _batchProcessorTask = Task.Run(BatchProcessorAsync);
    }

    /// <summary>
    /// Adds a log entry to the send queue.
    /// </summary>
    /// <param name="entry">The log entry to add.</param>
    public void AddLog(LogEntry entry)
    {
        if (_disposed)
        {
            return;
        }

        if (!_logQueue.TryAdd(entry))
        {
            Console.Error.WriteLine("LogBull: log queue full, dropping log");
        }
    }

    /// <summary>
    /// Immediately sends all queued logs.
    /// </summary>
    public void Flush()
    {
        SendBatch();
    }

    /// <summary>
    /// Stops the sender and sends all remaining logs.
    /// </summary>
    public void Shutdown()
    {
        if (_disposed)
        {
            return;
        }

        SendBatch();
        _cancellationTokenSource.Cancel();

        try
        {
            _batchProcessorTask.Wait(TimeSpan.FromSeconds(10));
        }
        catch (AggregateException)
        {
            // Ignore cancellation exceptions
        }
    }

    private async Task BatchProcessorAsync()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(BatchIntervalMs, _cancellationTokenSource.Token);
                SendBatch();
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"LogBull: batch processor error: {ex.Message}");
            }
        }

        // Send any remaining logs
        SendBatch();
    }

    private void SendBatch()
    {
        if (_disposed || _logQueue.Count == 0)
        {
            return;
        }

        var logs = new List<LogEntry>();
        while (logs.Count < BatchSize && _logQueue.TryTake(out var entry))
        {
            logs.Add(entry);
        }

        if (logs.Count == 0)
        {
            return;
        }

        // Try to acquire semaphore, if not available just submit anyway
        if (_workerSemaphore.Wait(0))
        {
            Task.Run(async () =>
            {
                try
                {
                    await SendHttpRequestAsync(logs);
                }
                finally
                {
                    _workerSemaphore.Release();
                }
            });
        }
        else
        {
            Task.Run(() => SendHttpRequestAsync(logs));
        }
    }

    private async Task SendHttpRequestAsync(List<LogEntry> logs)
    {
        var retries = 0;
        while (retries <= MaxRetries)
        {
            try
            {
                var batch = new { logs };
                var json = JsonSerializer.Serialize(batch);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{_config.Host}/api/v1/logs/receiving/{_config.ProjectId}";
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };

                request.Headers.Add("User-Agent", "LogBull-DotNet-Client/0.1.0");
                if (!string.IsNullOrEmpty(_config.ApiKey))
                {
                    request.Headers.Add("X-API-Key", _config.ApiKey);
                }

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var logBullResponse = JsonSerializer.Deserialize<LogBullResponse>(responseBody);
                        if (logBullResponse?.Rejected > 0)
                        {
                            HandleRejectedLogs(logBullResponse, logs);
                        }
                    }
                    catch
                    {
                        // Response parsing failed, but logs were accepted
                    }
                    return;
                }

                // Handle 5xx errors with retry
                if ((int)response.StatusCode >= 500)
                {
                    if (retries < MaxRetries)
                    {
                        retries++;
                        var delay = (int)Math.Pow(2, retries) * 1000; // Exponential backoff
                        await Task.Delay(delay);
                        continue;
                    }
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine(
                    $"LogBull: server returned status {(int)response.StatusCode}: {errorBody}");
                return;
            }
            catch (Exception ex)
            {
                if (retries < MaxRetries)
                {
                    retries++;
                    var delay = (int)Math.Pow(2, retries) * 1000; // Exponential backoff
                    await Task.Delay(delay);
                    continue;
                }

                Console.Error.WriteLine($"LogBull: HTTP request failed: {ex.Message}");
                return;
            }
        }
    }

    private void HandleRejectedLogs(LogBullResponse response, List<LogEntry> sentLogs)
    {
        Console.Error.WriteLine($"LogBull: Rejected {response.Rejected} log entries");

        if (response.Errors != null && response.Errors.Any())
        {
            Console.Error.WriteLine("LogBull: Rejected log details:");
            foreach (var error in response.Errors)
            {
                var index = error.Index;
                if (index >= 0 && index < sentLogs.Count)
                {
                    var log = sentLogs[index];
                    Console.Error.WriteLine($"  - Log #{index} rejected ({error.Message}):");
                    Console.Error.WriteLine($"    Level: {log.Level}");
                    Console.Error.WriteLine($"    Message: {log.Message}");
                    Console.Error.WriteLine($"    Timestamp: {log.Timestamp}");
                    if (log.Fields.Any())
                    {
                        var fieldsJson = JsonSerializer.Serialize(log.Fields);
                        Console.Error.WriteLine($"    Fields: {fieldsJson}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Disposes the sender and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Shutdown();

        _cancellationTokenSource.Dispose();
        _logQueue.Dispose();
        _workerSemaphore.Dispose();
        _httpClient.Dispose();
    }
}

