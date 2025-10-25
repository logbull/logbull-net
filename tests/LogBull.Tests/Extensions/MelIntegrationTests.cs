using System;
using LogBull.Core;
using LogBull.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LogBull.Tests.Extensions;

public class MelIntegrationTests
{
    [Fact]
    public void TestProviderCreation()
    {
        var config = Config.CreateBuilder()
            .WithProjectId("12345678-1234-1234-1234-123456789012")
            .WithHost("http://localhost:4005")
            .Build();

        var provider = new LogBullLoggerProvider(config);
        Assert.NotNull(provider);
        provider.Dispose();
    }

    [Fact]
    public void TestLoggerCreation()
    {
        var config = Config.CreateBuilder()
            .WithProjectId("12345678-1234-1234-1234-123456789012")
            .WithHost("http://localhost:4005")
            .Build();

        var provider = new LogBullLoggerProvider(config);
        var logger = provider.CreateLogger("TestCategory");

        Assert.NotNull(logger);
        provider.Dispose();
    }

    [Fact]
    public void TestLoggingBuilderExtension()
    {
        var config = Config.CreateBuilder()
            .WithProjectId("12345678-1234-1234-1234-123456789012")
            .WithHost("http://localhost:4005")
            .Build();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddLogBull(config);
        });

        var logger = loggerFactory.CreateLogger<MelIntegrationTests>();
        Assert.NotNull(logger);

        logger.LogInformation("Test message");
        loggerFactory.Dispose();
    }

    [Fact]
    public void TestLoggingBuilderExtensionWithAction()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddLogBull(configBuilder =>
            {
                configBuilder
                    .WithProjectId("12345678-1234-1234-1234-123456789012")
                    .WithHost("http://localhost:4005")
                    .WithLogLevel(LogBull.Core.LogLevel.DEBUG);
            });
        });

        var logger = loggerFactory.CreateLogger<MelIntegrationTests>();
        Assert.NotNull(logger);

        logger.LogDebug("Debug message");
        logger.LogInformation("Info message");
        loggerFactory.Dispose();
    }

    [Fact]
    public void TestStructuredLogging()
    {
        var config = Config.CreateBuilder()
            .WithProjectId("12345678-1234-1234-1234-123456789012")
            .WithHost("http://localhost:4005")
            .Build();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddLogBull(config);
        });

        var logger = loggerFactory.CreateLogger<MelIntegrationTests>();

        logger.LogInformation("User {UserId} performed action {Action}", "12345", "login");
        loggerFactory.Dispose();
    }

    [Fact]
    public void TestLevelMapping()
    {
        var config = Config.CreateBuilder()
            .WithProjectId("12345678-1234-1234-1234-123456789012")
            .WithHost("http://localhost:4005")
            .WithLogLevel(LogBull.Core.LogLevel.DEBUG)
            .Build();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddLogBull(config);
        });

        var logger = loggerFactory.CreateLogger<MelIntegrationTests>();

        // Test all log levels
        logger.LogTrace("Trace message");
        logger.LogDebug("Debug message");
        logger.LogInformation("Information message");
        logger.LogWarning("Warning message");
        logger.LogError("Error message");
        logger.LogCritical("Critical message");

        loggerFactory.Dispose();
    }

    [Fact]
    public void TestExceptionLogging()
    {
        var config = Config.CreateBuilder()
            .WithProjectId("12345678-1234-1234-1234-123456789012")
            .WithHost("http://localhost:4005")
            .Build();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddLogBull(config);
        });

        var logger = loggerFactory.CreateLogger<MelIntegrationTests>();
        var exception = new InvalidOperationException("Test exception");

        logger.LogError(exception, "An error occurred");
        loggerFactory.Dispose();
    }
}

