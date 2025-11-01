using JsonPath.Parser;
using JsonPath.Parser.Lexer;
using Xunit;

namespace Json.Path.Tests.Lexing;

public class IdentifierScannerTests
{
    [Theory]
    [InlineData("foo", "foo")]
    [InlineData("_bar", "_bar")]
    [InlineData("alpha123", "alpha123")]
    [InlineData("name_with_underscores", "name_with_underscores")]
    public void Should_Scan_ValidIdentifier(string input, string expected)
    {
        var ctx = new ScanContext(input.AsSpan());
        var scanner = new IdentifierScanner();

        var result = scanner.TryScan(ref ctx, out var token);

        Assert.True(result);
        Assert.Equal(TokenKind.Identifier, token.Kind);
        Assert.Equal(0, token.Start);
        Assert.Equal(expected.Length, token.Length);
        Assert.Equal(expected, new string(token.Slice(input)));
        Assert.Equal(expected.Length, ctx.Position);
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
        Assert.Equal("foo", new string(token.Slice(input)));
        Assert.Equal(3, ctx.Position);
    }

    [Theory]
    [InlineData("1alpha")]
    [InlineData("-invalid")]
    [InlineData("\u2603snowman")]
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
