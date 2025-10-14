using System;
using System.Collections.Generic;
using LogBull.Core;
using Xunit;

namespace LogBull.Tests;

public class LogBullLoggerTests
{
    [Fact]
    public void TestCreateLogger()
    {
        var config = Config.CreateBuilder()
            .WithProjectId("12345678-1234-1234-1234-123456789012")
            .WithHost("http://localhost:4005")
            .Build();

        var logger = LogBullLogger.Create(config);
        Assert.NotNull(logger);
        logger.Dispose();
    }

    [Fact]
    public void TestBuilderPattern()
    {
        var logger = LogBullLogger.CreateBuilder()
            .WithProjectId("12345678-1234-1234-1234-123456789012")
            .WithHost("http://localhost:4005")
            .WithApiKey("test-api-key")
            .WithLogLevel(LogLevel.DEBUG)
            .Build();

        Assert.NotNull(logger);
        logger.Dispose();
    }

    [Fact]
    public void TestInvalidProjectId()
    {
        Assert.Throws<ArgumentException>(() => 
            LogBullLogger.CreateBuilder()
                .WithProjectId("invalid")
                .WithHost("http://localhost:4005")
                .Build());
    }

    [Fact]
    public void TestInvalidHost()
    {
        Assert.Throws<ArgumentException>(() => 
            LogBullLogger.CreateBuilder()
                .WithProjectId("12345678-1234-1234-1234-123456789012")
                .WithHost("invalid")
                .Build());
    }

    [Fact]
    public void TestLogMethods()
    {
        var logger = LogBullLogger.CreateBuilder()
            .WithProjectId("12345678-1234-1234-1234-123456789012")
            .WithHost("http://localhost:4005")
            .WithLogLevel(LogLevel.DEBUG)
            .Build();

        // Should not throw exceptions
        logger.Debug("debug message");
        logger.Info("info message");
        logger.Warning("warning message");
        logger.Error("error message");
        logger.Critical("critical message");

        logger.Dispose();
    }

    [Fact]
    public void TestLogWithFields()
    {
        var logger = LogBullLogger.CreateBuilder()
            .WithProjectId("12345678-1234-1234-1234-123456789012")
            .WithHost("http://localhost:4005")
            .Build();

        var fields = new Dictionary<string, object>
        {
            { "user_id", "12345" },
            { "action", "login" },
            { "count", 42 }
        };

        logger.Info("User action", fields);
        logger.Dispose();
    }

    [Fact]
    public void TestWithContext()
    {
        var logger = LogBullLogger.CreateBuilder()
            .WithProjectId("12345678-1234-1234-1234-123456789012")
            .WithHost("http://localhost:4005")
            .Build();

        var context = new Dictionary<string, object>
        {
            { "session_id", "sess_123" },
            { "user_id", "user_456" }
        };

        var contextLogger = logger.WithContext(context);
        Assert.NotNull(contextLogger);

        contextLogger.Info("Context test");
        logger.Dispose();
    }

    [Fact]
    public void TestFlush()
    {
        var logger = LogBullLogger.CreateBuilder()
            .WithProjectId("12345678-1234-1234-1234-123456789012")
            .WithHost("http://localhost:4005")
            .Build();

        logger.Info("Test message");
        logger.Flush();
        logger.Dispose();
    }

    [Fact]
    public void TestShutdown()
    {
        var logger = LogBullLogger.CreateBuilder()
            .WithProjectId("12345678-1234-1234-1234-123456789012")
            .WithHost("http://localhost:4005")
            .Build();

        logger.Info("Test message");
        logger.Shutdown();

        // Should not throw exception after shutdown
        logger.Info("After shutdown");
        logger.Dispose();
    }

    [Fact]
    public void TestLevelFiltering()
    {
        var logger = LogBullLogger.CreateBuilder()
            .WithProjectId("12345678-1234-1234-1234-123456789012")
            .WithHost("http://localhost:4005")
            .WithLogLevel(LogLevel.WARNING)
            .Build();

        // These should be filtered out (no exceptions should be thrown)
        logger.Debug("debug message");
        logger.Info("info message");

        // These should pass through
        logger.Warning("warning message");
        logger.Error("error message");
        logger.Critical("critical message");

        logger.Dispose();
    }
}

