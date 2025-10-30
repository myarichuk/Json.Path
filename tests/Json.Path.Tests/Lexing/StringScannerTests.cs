using JsonPath.Parser.Lexer;
using Xunit;

namespace JsonPath.Tests.Lexer;

public class StringScannerTests
{
    private static ScanContext CreateContext(string input) => new(input.AsSpan());

    [Theory]
    [InlineData("\"")]
    [InlineData("'")]
    [InlineData("\"foobar")]
    [InlineData("'foobar")]
    [InlineData("foobar'")]
    [InlineData("'foobar\\\"")]
    [InlineData("\"foobar'")]
    [InlineData("AaA\"foobar\"")]
    [InlineData("'fo\"")]
    public void ShouldNotMatch_MalformedStrings(string input)
    {
        var ctx = CreateContext(input);
        var subscanner = new StringScanner();

        var result = subscanner.TryScan(ref ctx, out _);
        Assert.False(result);
        Assert.Equal(0, ctx.Position); // do not consume if no match!
    }

    [Theory]
    [InlineData("'test'", 1, 4)]
    [InlineData("'a'", 1, 1)] // edge case
    [InlineData("'foobar'", 1, 6)]
    [InlineData("\"foobar\"", 1, 6)]
    [InlineData("\"a\"", 1, 1)] // edge case
    [InlineData("\"foo\\\"bar\"", 1, 8)] // include escaped character
    [InlineData("''", 1, 0)]
    [InlineData("\"\"", 1, 0)]
    public void CanMatch_ProperStrings(
        string input,
        int expectedTokenStart,
        int expectedLength)
    {
        var ctx = CreateContext(input);
        var subscanner = new StringScanner();

        var result = subscanner.TryScan(ref ctx, out var token);
        Assert.True(result);
        Assert.Equal(expectedTokenStart, token.Start);
        Assert.Equal(expectedLength, token.Length);
    }

    [Theory]
    [InlineData("foo123\"string_content\"", 14)]
    [InlineData("foo123\"a\\\"b\"", 4)]
    [InlineData("foo123\"a__b\"", 4)]
    [InlineData("foo123\"a\a\bb\"", 4)]
    public void CanMatch_Not_FromStart(string input, int expecteLength)
    {
        var ctx = CreateContext(input);
        ctx.Consume(6); // simulate mid-lexing
        var subscanner = new StringScanner();

        var result = subscanner.TryScan(ref ctx, out var token);
        Assert.True(result);
        Assert.Equal(7, token.Start);
        Assert.Equal(expecteLength, token.Length);
    }
}