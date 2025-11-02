using JsonPath.Parser.Lexer;
using Xunit;

namespace Json.Path.Tests.Lexing;

public class TokenScannerTests
{
    private static ScanContext CreateContext(string input) => new(input.AsSpan());

    [Fact]
    public void MatchesLiteral_AtStart()
    {
        var ctx = CreateContext("$.store");
        var subscanner = new TokenScanner("$", TokenKind.Root);

        var result = subscanner.TryScan(ref ctx, out var token);

        Assert.True(result);
        Assert.Equal(TokenKind.Root, token.Kind);
        Assert.Equal(0, token.Start);
        Assert.Equal(1, token.Length);
        Assert.Equal(1, ctx.Position);  // consumed
    }

    [Fact]
    public void DoesNotMatch_DifferentCharacter()
    {
        var ctx = CreateContext("a.store");
        var subscanner = new TokenScanner("$", TokenKind.Root);

        var result = subscanner.TryScan(ref ctx, out var token);

        Assert.False(result);
        Assert.Equal(default, token);
        Assert.Equal(0, ctx.Position); // unchanged
    }

    [Fact]
    public void ShouldMatch_Multicharacter()
    {
        var ctx = CreateContext("..book");
        var subscanner = new TokenScanner("..", TokenKind.DotDot);

        var result = subscanner.TryScan(ref ctx, out var token);

        Assert.True(result);
        Assert.Equal(TokenKind.DotDot, token.Kind);
        Assert.Equal(0, token.Start);
        Assert.Equal(2, token.Length);
        Assert.Equal(2, ctx.Position);
    }

    [Fact]
    public void DoesNotMatch_BadPrefix()
    {
        var ctx = CreateContext(".book");
        var subscanner = new TokenScanner("..", TokenKind.DotDot);

        var result = subscanner.TryScan(ref ctx, out var token);

        Assert.False(result);
        Assert.Equal(default, token);
        Assert.Equal(0, ctx.Position);
    }

    [Fact]
    public void DoesNotMatch_InputTooShort()
    {
        var ctx = CreateContext(".");
        var subscanner = new TokenScanner("..", TokenKind.DotDot);

        var result = subscanner.TryScan(ref ctx, out var token);

        Assert.False(result);
        Assert.Equal(default, token);
        Assert.Equal(0, ctx.Position);
    }

    [Fact]
    public void Should_ConsumeContextIfNeeded()
    {
        var ctx = CreateContext("$$");
        var subscanner = new TokenScanner("$", TokenKind.Root);

        Assert.True(subscanner.TryScan(ref ctx, out _));
        Assert.True(subscanner.TryScan(ref ctx, out _));
        Assert.Equal(2, ctx.Position);
    }
}