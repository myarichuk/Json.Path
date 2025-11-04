using JsonPath.Parser;
using JsonPath.Parser.Lexer;
using Xunit;

namespace Json.Path.Tests.Lexing;

public class IdentifierScannerTests
{
    [Theory]
    [InlineData("foo", 0, "foo")]
    [InlineData("_bar", 0, "_bar")]
    [InlineData("alpha123", 0, "alpha123")]
    [InlineData("alpha123", 2, "pha123")]
    [InlineData("name_with_underscores", 0, "name_with_underscores")]
    [InlineData("name_with_underscores", 3, "e_with_underscores")]

    public void Should_Scan_ValidIdentifier(string input, int preConsume, string expected)
    {
        var ctx = new ScanContext(input.AsSpan());
        var scanner = new IdentifierScanner();
        if (preConsume > 0)
        {
            ctx.Consume(preConsume);
        }

        var result = scanner.TryScan(ref ctx, out var token);

        Assert.True(result);
        Assert.Equal(TokenKind.Identifier, token.Kind);
        Assert.Equal(preConsume, token.Start);
        Assert.Equal(expected.Length, token.Length);
        Assert.Equal(expected, ctx.SliceFrom(token));
        Assert.Equal(expected.Length, ctx.Position - preConsume);
    }

    [Fact]
    public void Should_StopAt_FirstNonIdentifierCharacter()
    {
        var input = "foo-bar";
        var ctx = new ScanContext(input.AsSpan());
        var scanner = new IdentifierScanner();

        var result = scanner.TryScan(ref ctx, out var token);

        Assert.True(result);
        Assert.Equal(0, token.Start);
        Assert.Equal(3, token.Length);
        Assert.Equal("foo", new string(token.SliceFrom(input)));
        Assert.Equal(3, ctx.Position);
    }

    [Theory]
    [InlineData("1foobar")]
    [InlineData("-invalid")]
    [InlineData("\u2603foobar")]
    [InlineData("")]
    public void Should_Reject_WhenFirstCharInvalid(string input)
    {
        var ctx = new ScanContext(input.AsSpan());
        var scanner = new IdentifierScanner();

        var result = scanner.TryScan(ref ctx, out var token);

        Assert.False(result);
        Assert.Equal(default, token);
        Assert.Equal(0, ctx.Position);
    }

    [Fact]
    public void Should_Handle_MixedCaseAndDigits()
    {
        var input = "JsonPath2";
        var ctx = new ScanContext(input.AsSpan());
        var scanner = new IdentifierScanner();

        Assert.True(scanner.TryScan(ref ctx, out var token));
        Assert.Equal(TokenKind.Identifier, token.Kind);
        Assert.Equal(input.Length, token.Length);
        Assert.Equal(input.Length, ctx.Position);
    }
}
