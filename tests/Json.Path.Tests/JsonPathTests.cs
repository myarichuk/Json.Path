using System;
using System.Text.Json;
using Xunit;

namespace Json.Path.Tests;

public sealed class JsonPathTests
{
    private static readonly JsonElementNavigator Navigator = new();

    [Theory]
    [InlineData("$.name", "Alice")]
    [InlineData("$.details.city", "Paris")]
    public void TryResolve_ReturnsPropertyValue(string path, string expected)
    {
        var json = JsonDocument.Parse("""{"name":"Alice","details":{"city":"Paris"}}""");

        Assert.True(JsonPath.TryResolve(json.RootElement, path, Navigator, out var result));
        Assert.Equal(expected, result.GetString());
    }

    [Fact]
    public void TryResolve_ReturnsArrayElement()
    {
        var json = JsonDocument.Parse("""{"items":[{"id":1},{"id":2}]}""");

        Assert.True(JsonPath.TryResolve(json.RootElement, "$.items[1].id", Navigator, out var result));
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

        Assert.False(JsonPath.TryResolve(json.RootElement, path, Navigator, out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("$items")]
    [InlineData("$.")]
    [InlineData("$.items[-1]")]
    public void TryCompile_InvalidExpressionsReturnFalse(string path)
    {
        Assert.False(JsonPath.TryCompile(path, out _));
    }

    [Fact]
    public void CompiledPath_IsReusable()
    {
        const string Path = "$.items[0].name";
        var json = JsonDocument.Parse("""{"items":[{"name":"One"},{"name":"Two"}]}""");

        Assert.True(JsonPath.TryCompile(Path, out var program));
        Assert.True(program.TryEvaluate(json.RootElement, Navigator, out var result));
        Assert.Equal("One", result.GetString());

        Assert.True(program.TryEvaluate(json.RootElement, Navigator, out var repeat));
        Assert.Equal("One", repeat.GetString());
    }

    [Fact]
    public void TryCompile_RootExpressionReturnsRoot()
    {
        var json = JsonDocument.Parse("""{"name":"root"}""");

        Assert.True(JsonPath.TryCompile("$", out var program));
        Assert.True(program.TryEvaluate(json.RootElement, Navigator, out var result));
        Assert.Equal("root", result.GetProperty("name").GetString());
    }

    private sealed class JsonElementNavigator : IJsonNavigator<JsonElement>
    {
        public bool TryGetProperty(JsonElement node, ReadOnlySpan<char> propertyName, out JsonElement value)
        {
            if (node.ValueKind != JsonValueKind.Object)
            {
                value = default;
                return false;
            }

            return node.TryGetProperty(propertyName.ToString(), out value);
        }

        public bool TryGetElement(JsonElement node, int index, out JsonElement value)
        {
            if (node.ValueKind != JsonValueKind.Array || index < 0)
            {
                value = default;
                return false;
            }

            var enumerator = node.EnumerateArray();
            var current = 0;
            foreach (var element in enumerator)
            {
                if (current == index)
                {
                    value = element;
                    return true;
                }

                current++;
            }

            value = default;
            return false;
        }
    }
}
