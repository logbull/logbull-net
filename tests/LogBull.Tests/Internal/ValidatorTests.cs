using System;
using System.Collections.Generic;
using LogBull.Internal.Validation;
using Xunit;

namespace LogBull.Tests.Internal;

public class ValidatorTests
{
    private readonly Validator _validator = new();

    [Fact]
    public void TestValidProjectId()
    {
        var exception = Record.Exception(() =>
            _validator.ValidateProjectId("12345678-1234-1234-1234-123456789012"));
        Assert.Null(exception);
    }

    [Fact]
    public void TestInvalidProjectIdFormat()
    {
        Assert.Throws<ArgumentException>(() =>
            _validator.ValidateProjectId("invalid"));
    }

    [Fact]
    public void TestEmptyProjectId()
    {
        Assert.Throws<ArgumentException>(() =>
            _validator.ValidateProjectId(""));
    }

    [Fact]
    public void TestValidHttpUrl()
    {
        var exception = Record.Exception(() =>
            _validator.ValidateHostUrl("http://localhost:4005"));
        Assert.Null(exception);
    }

    [Fact]
    public void TestValidHttpsUrl()
    {
        var exception = Record.Exception(() =>
            _validator.ValidateHostUrl("https://logbull.example.com"));
        Assert.Null(exception);
    }

    [Fact]
    public void TestInvalidUrlScheme()
    {
        Assert.Throws<ArgumentException>(() =>
            _validator.ValidateHostUrl("ftp://example.com"));
    }

    [Fact]
    public void TestEmptyUrl()
    {
        Assert.Throws<ArgumentException>(() =>
            _validator.ValidateHostUrl(""));
    }

    [Fact]
    public void TestValidApiKey()
    {
        var exception = Record.Exception(() =>
            _validator.ValidateApiKey("abc123_xyz-789.test"));
        Assert.Null(exception);
    }

    [Fact]
    public void TestShortApiKey()
    {
        Assert.Throws<ArgumentException>(() =>
            _validator.ValidateApiKey("short"));
    }

    [Fact]
    public void TestInvalidApiKeyCharacters()
    {
        Assert.Throws<ArgumentException>(() =>
            _validator.ValidateApiKey("invalid@key!"));
    }

    [Fact]
    public void TestNullApiKey()
    {
        var exception = Record.Exception(() =>
            _validator.ValidateApiKey(null));
        Assert.Null(exception);
    }

    [Fact]
    public void TestValidLogMessage()
    {
        var exception = Record.Exception(() =>
            _validator.ValidateLogMessage("This is a valid log message"));
        Assert.Null(exception);
    }

    [Fact]
    public void TestEmptyLogMessage()
    {
        Assert.Throws<ArgumentException>(() =>
            _validator.ValidateLogMessage(""));
    }

    [Fact]
    public void TestTooLongLogMessage()
    {
        var longMessage = new string('a', 10_001);
        Assert.Throws<ArgumentException>(() =>
            _validator.ValidateLogMessage(longMessage));
    }

    [Fact]
    public void TestValidFields()
    {
        var fields = new Dictionary<string, object>
        {
            { "user_id", "12345" },
            { "action", "login" }
        };

        var exception = Record.Exception(() =>
            _validator.ValidateLogFields(fields));
        Assert.Null(exception);
    }

    [Fact]
    public void TestTooManyFields()
    {
        var fields = new Dictionary<string, object>();
        for (int i = 0; i < 101; i++)
        {
            fields[$"field_{i}"] = i;
        }

        Assert.Throws<ArgumentException>(() =>
            _validator.ValidateLogFields(fields));
    }

    [Fact]
    public void TestEmptyFieldKey()
    {
        var fields = new Dictionary<string, object> { { "", "value" } };
        Assert.Throws<ArgumentException>(() =>
            _validator.ValidateLogFields(fields));
    }

    [Fact]
    public void TestTooLongFieldKey()
    {
        var fields = new Dictionary<string, object>
        {
            { new string('a', 101), "value" }
        };
        Assert.Throws<ArgumentException>(() =>
            _validator.ValidateLogFields(fields));
    }
}

