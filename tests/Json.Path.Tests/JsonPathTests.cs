using System.Text.Json;
using Xunit;

namespace Json.Path.Tests;

public sealed class JsonPathTests
{
    [Theory]
    [InlineData("$.name", "Alice")]
    [InlineData("$.details.city", "Paris")]
    public void TryResolve_ReturnsPropertyValue(string path, string expected)
    {
        var json = JsonDocument.Parse("""{"name":"Alice","details":{"city":"Paris"}}""");

        Assert.True(JsonPath.TryResolve(json.RootElement, path, out var result));
        Assert.Equal(expected, result.GetString());
    }

    [Fact]
    public void TryResolve_ReturnsArrayElement()
    {
        var json = JsonDocument.Parse("""{"items":[{"id":1},{"id":2}]}""");

        Assert.True(JsonPath.TryResolve(json.RootElement, "$.items[1].id", out var result));
        Assert.Equal(2, result.GetInt32());
    }

    [Theory]
    [InlineData("$.missing")]
    [InlineData("$.items[4]")]
    [InlineData("$items")]
    [InlineData("$.items[-1]")]
    public void TryResolve_InvalidPathsReturnFalse(string path)
    {
        var json = JsonDocument.Parse("""{"items":[1,2,3]}""");

        Assert.False(JsonPath.TryResolve(json.RootElement, path, out _));
    }
}
