using System.Collections.Generic;
using LogBull.Internal.Formatting;
using Xunit;

namespace LogBull.Tests.Internal;

public class FormatterTests
{
    private readonly Formatter _formatter = new();

    [Fact]
    public void TestFormatMessage()
    {
        var message = "  Test message  ";
        var formatted = _formatter.FormatMessage(message);
        Assert.Equal("Test message", formatted);
    }

    [Fact]
    public void TestFormatLongMessage()
    {
        var longMessage = new string('a', 15_000);
        var formatted = _formatter.FormatMessage(longMessage);
        Assert.True(formatted.Length <= 10_000);
        Assert.EndsWith("...", formatted);
    }

    [Fact]
    public void TestFormatNullMessage()
    {
        var formatted = _formatter.FormatMessage(null);
        Assert.Equal(string.Empty, formatted);
    }

    [Fact]
    public void TestEnsureFields()
    {
        var fields = new Dictionary<string, object>
        {
            { "user_id", "12345" },
            { "count", 42 }
        };

        var ensured = _formatter.EnsureFields(fields);
        Assert.Equal(2, ensured.Count);
        Assert.Equal("12345", ensured["user_id"]);
        Assert.Equal(42, ensured["count"]);
    }

    [Fact]
    public void TestEnsureNullFields()
    {
        var ensured = _formatter.EnsureFields(null);
        Assert.NotNull(ensured);
        Assert.Empty(ensured);
    }

    [Fact]
    public void TestEnsureFieldsWithEmptyKeys()
    {
        var fields = new Dictionary<string, object>
        {
            { "", "should be skipped" },
            { "  ", "should be skipped" },
            { "valid", "should be kept" }
        };

        var ensured = _formatter.EnsureFields(fields);
        Assert.Equal(1, ensured.Count);
        Assert.True(ensured.ContainsKey("valid"));
    }

    [Fact]
    public void TestMergeFields()
    {
        var baseFields = new Dictionary<string, object>
        {
            { "a", 1 },
            { "b", 2 }
        };

        var additionalFields = new Dictionary<string, object>
        {
            { "c", 3 },
            { "b", 20 }
        };

        var merged = _formatter.MergeFields(baseFields, additionalFields);
        Assert.Equal(3, merged.Count);
        Assert.Equal(1, merged["a"]);
        Assert.Equal(20, merged["b"]); // Should be overridden
        Assert.Equal(3, merged["c"]);
    }

    [Fact]
    public void TestMergeWithNullBase()
    {
        var additionalFields = new Dictionary<string, object> { { "a", 1 } };
        var merged = _formatter.MergeFields(null, additionalFields);
        Assert.Equal(1, merged.Count);
        Assert.Equal(1, merged["a"]);
    }

    [Fact]
    public void TestMergeWithNullAdditional()
    {
        var baseFields = new Dictionary<string, object> { { "a", 1 } };
        var merged = _formatter.MergeFields(baseFields, null);
        Assert.Equal(1, merged.Count);
        Assert.Equal(1, merged["a"]);
    }
}

