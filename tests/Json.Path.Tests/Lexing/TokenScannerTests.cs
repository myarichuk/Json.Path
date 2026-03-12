using JsonPath.Parser.Allocators;
using JsonPath.Parser.Diagnostics;
using JsonPath.Parser.Helpers;
using JsonPath.Parser.Lexer;
using Xunit;

namespace Json.Path.Tests.Lexing;

public class TokenScannerTests
{
    private static void CreateContext(
        string input,
        out ScanContext ctx,
        out ArenaAllocator allocator,
        out ArenaList<JsonPathError> errors)
    {
        allocator = new ArenaAllocator();
        ctx = new ScanContext(input.AsSpan());
        errors = new ArenaList<JsonPathError>(allocator);
    }

    [Fact]
    public void MatchesLiteral_AtStart()
    {
        CreateContext("$.store", out var ctx, out var allocator, out var errors);
        try
        {
            var subscanner = new TokenScanner("$", TokenKind.Root);

            var result = subscanner.TryScan(ref ctx, allocator, errors, out var token);

            Assert.True(result);
            Assert.True(errors.IsEmpty);
            Assert.Equal(TokenKind.Root, token.Kind);
            Assert.Equal(0, token.Start);
            Assert.Equal(1, token.Length);
            Assert.Equal(1, ctx.Position);  // consumed
        }
        finally
        {
            allocator.Dispose();
        }
    }

    [Fact]
    public void DoesNotMatch_DifferentCharacter()
    {
        CreateContext("a.store", out var ctx, out var allocator, out var errors);
        try
        {
            var subscanner = new TokenScanner("$", TokenKind.Root);

            var result = subscanner.TryScan(ref ctx, allocator, errors, out var token);

            Assert.False(result);
            Assert.True(errors.IsEmpty);
            Assert.Equal(default, token);
            Assert.Equal(0, ctx.Position); // unchanged
        }
        finally
        {
            allocator.Dispose();
        }
    }

    [Fact]
    public void ShouldMatch_Multicharacter()
    {
        CreateContext("..book", out var ctx, out var allocator, out var errors);
        try
        {
            var subscanner = new TokenScanner("..", TokenKind.DotDot);

            var result = subscanner.TryScan(ref ctx, allocator, errors, out var token);

            Assert.True(result);
            Assert.True(errors.IsEmpty);
            Assert.Equal(TokenKind.DotDot, token.Kind);
            Assert.Equal(0, token.Start);
            Assert.Equal(2, token.Length);
            Assert.Equal(2, ctx.Position);
        }
        finally
        {
            allocator.Dispose();
        }
    }

    [Fact]
    public void DoesNotMatch_BadPrefix()
    {
        CreateContext(".book", out var ctx, out var allocator, out var errors);
        try
        {
            var subscanner = new TokenScanner("..", TokenKind.DotDot);

            var result = subscanner.TryScan(ref ctx, allocator, errors, out var token);

            Assert.False(result);
            Assert.True(errors.IsEmpty);
            Assert.Equal(default, token);
            Assert.Equal(0, ctx.Position);
        }
        finally
        {
            allocator.Dispose();
        }
    }

    [Fact]
    public void DoesNotMatch_InputTooShort()
    {
        CreateContext(".", out var ctx, out var allocator, out var errors);
        try
        {
            var subscanner = new TokenScanner("..", TokenKind.DotDot);

            var result = subscanner.TryScan(ref ctx, allocator, errors, out var token);

            Assert.False(result);
            Assert.True(errors.IsEmpty);
            Assert.Equal(default, token);
            Assert.Equal(0, ctx.Position);
        }
        finally
        {
            allocator.Dispose();
        }
    }

    [Fact]
    public void Should_ConsumeContextIfNeeded()
    {
        CreateContext("$$", out var ctx, out var allocator, out var errors);
        try
        {
            var subscanner = new TokenScanner("$", TokenKind.Root);

            Assert.True(subscanner.TryScan(ref ctx, allocator, errors, out _));
            Assert.True(errors.IsEmpty);
            Assert.True(subscanner.TryScan(ref ctx, allocator, errors, out _));
            Assert.True(errors.IsEmpty);
            Assert.Equal(2, ctx.Position);
        }
        finally
        {
            allocator.Dispose();
        }
    }
}