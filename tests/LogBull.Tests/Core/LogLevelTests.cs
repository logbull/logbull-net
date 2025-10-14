using LogBull.Core;
using Xunit;

namespace LogBull.Tests.Core;

public class LogLevelTests
{
    [Fact]
    public void TestLogLevelPriorities()
    {
        Assert.True(LogLevel.DEBUG.GetPriority() < LogLevel.INFO.GetPriority());
        Assert.True(LogLevel.INFO.GetPriority() < LogLevel.WARNING.GetPriority());
        Assert.True(LogLevel.WARNING.GetPriority() < LogLevel.ERROR.GetPriority());
        Assert.True(LogLevel.ERROR.GetPriority() < LogLevel.CRITICAL.GetPriority());
    }

    [Fact]
    public void TestLogLevelToString()
    {
        Assert.Equal("DEBUG", LogLevel.DEBUG.ToString());
        Assert.Equal("INFO", LogLevel.INFO.ToString());
        Assert.Equal("WARNING", LogLevel.WARNING.ToString());
        Assert.Equal("ERROR", LogLevel.ERROR.ToString());
        Assert.Equal("CRITICAL", LogLevel.CRITICAL.ToString());
    }

    [Fact]
    public void TestLogLevelValues()
    {
        Assert.Equal(10, LogLevel.DEBUG.GetPriority());
        Assert.Equal(20, LogLevel.INFO.GetPriority());
        Assert.Equal(30, LogLevel.WARNING.GetPriority());
        Assert.Equal(40, LogLevel.ERROR.GetPriority());
        Assert.Equal(50, LogLevel.CRITICAL.GetPriority());
    }

    [Fact]
    public void TestPriorityOrdering()
    {
        var levels = new[] { LogLevel.DEBUG, LogLevel.INFO, LogLevel.WARNING, LogLevel.ERROR, LogLevel.CRITICAL };
        
        for (int i = 0; i < levels.Length - 1; i++)
        {
            Assert.True(levels[i].GetPriority() < levels[i + 1].GetPriority());
        }
    }
}

