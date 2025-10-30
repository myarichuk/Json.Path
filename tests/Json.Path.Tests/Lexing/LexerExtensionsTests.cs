using JsonPath.Parser.Lexer;
using Xunit;

namespace Json.Path.Tests.Lexing;

public class LexerExtensionsTests
{
    [Fact]
    public void TryPeekForLiteral_ShouldMatchLiteralAtCurrentPosition()
    {
        var input = "foobar".AsSpan();
        var ctx = new ScanContext(input);

        var success = ctx.TryPeekForLiteral(0, "foo".AsSpan(), out var token);

        Assert.True(success);
        Assert.Equal(0, token.Start);
        Assert.Equal(3, token.Length);
        Assert.Equal(1, token.Line);
        Assert.Equal(1, token.Column);
    }

    [Fact]
    public void TryPeekForLiteral_ShouldRespectOffset()
    {
        var input = "  foo".AsSpan();
        var ctx = new ScanContext(input);

        var success = ctx.TryPeekForLiteral(2, "foo".AsSpan(), out var token);

        Assert.True(success);
        Assert.Equal(2, token.Start);
        Assert.Equal(3, token.Length);
    }

    [Fact]
    public void TryPeekForLiteral_ShouldFailIfNotEnoughRemainingChars()
    {
        var input = "foo".AsSpan();
        var ctx = new ScanContext(input);

        var success = ctx.TryPeekForLiteral(2, "bar".AsSpan(), out var token);

        Assert.False(success);
        Assert.Equal(default, token);
    }

    [Fact]
    public void TryPeekUntil_ShouldFindSingleCharDelimiter()
    {
        var input = "abc:def".AsSpan();
        var ctx = new ScanContext(input);

        var success = ctx.TryPeekUntil(0, ":", out var token);

        Assert.True(success);
        Assert.Equal(0, token.Start);
        Assert.Equal(3, token.Length);
    }

    [Fact]
    public void TryPeekUntil_ShouldFindMultiCharDelimiter()
    {
        var input = "some text END more".AsSpan();
        var ctx = new ScanContext(input);

        var success = ctx.TryPeekUntil(0, "END".AsSpan(), out var token);

        Assert.True(success);
        Assert.Equal(0, token.Start);
        Assert.Equal(10, token.Length);
    }

    [Fact]
    public void TryPeekUntil_ShouldFailIfDelimiterNotFound()
    {
        var input = "some text without end".AsSpan();
        var ctx = new ScanContext(input);

        var success = ctx.TryPeekUntil(0, "XYZ".AsSpan(), out var token);

        Assert.False(success);
        Assert.Equal(default, token);
    }

    [Fact]
    public void TryPeekUntil_ShouldRespectOffset()
    {
        var input = "ignore-this then stop here".AsSpan();
        var ctx = new ScanContext(input);

        var success = ctx.TryPeekUntil(12, "stop".AsSpan(), out var token);

        Assert.True(success);
        Assert.Equal(12, token.Start);
        Assert.Equal(5, token.Length);
    }

    [Fact]
    public void TryPeekUntil_ShouldHandleEmptyDelimiter()
    {
        var input = "abc".AsSpan();
        var ctx = new ScanContext(input);

        var success = ctx.TryPeekUntil(0, ReadOnlySpan<char>.Empty, out var token);

        Assert.False(success);
        Assert.Equal(default, token);
    }
}