using JsonPath.Parser;
using JsonPath.Parser.Helpers;
using JsonPath.Parser.Lexer;
using Xunit;

namespace Json.Path.Tests.Helpers;

public class ArenaListTests : IDisposable
{
    // small to force growth quickly
    private readonly ArenaAllocator _arena = new(1024);

    [Fact]
    public void Add_SingleToken_ShouldStoreCorrectly()
    {
        var list = new ArenaList<Token>(_arena);
        var tok = new Token(TokenKind.Dot, 5, 1);

        list.Add(tok);

        var span = list.AsSpan();
        Assert.Equal(1, span.Length);
        Assert.Equal(TokenKind.Dot, span[0].Kind);
        Assert.Equal(5, span[0].Start);
    }

    [Fact]
    public void Add_MultipleTokens_ShouldBeSequential()
    {
        var list = new ArenaList<Token>(_arena);
        for (int i = 0; i < 10; i++)
        {
            list.Add(new Token(TokenKind.Number, i * 2, 1));
        }

        var span = list.AsSpan();
        Assert.Equal(10, span.Length);

        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(TokenKind.Number, span[i].Kind);
            Assert.Equal(i * 2, span[i].Start);
        }
    }

    [Fact]
    public void Add_WhenExceedingCapacity_ShouldGrowAndPreserveContents()
    {
        var list = new ArenaList<Token>(_arena, initialCapacity: 2);
        list.Add(new Token(TokenKind.Root, 0, 1));
        list.Add(new Token(TokenKind.Current, 1, 1));

        // grow here
        list.Add(new Token(TokenKind.Dot, 2, 1));

        var span = list.AsSpan();
        Assert.Equal(3, span.Length);
        Assert.Equal(TokenKind.Root, span[0].Kind);
        Assert.Equal(TokenKind.Current, span[1].Kind);
        Assert.Equal(TokenKind.Dot, span[2].Kind);
    }

    [Fact]
    public void AsSpan_ShouldReflectAllAddedTokens()
    {
        var list = new ArenaList<Token>(_arena, initialCapacity: 4);
        for (int i = 0; i < 4; i++)
        {
            list.Add(new Token(TokenKind.Identifier, i, 1));
        }

        var span = list.AsSpan();
        Assert.Equal(4, span.Length);

        for (int i = 0; i < 4; i++)
        {
            Assert.Equal(i, span[i].Start);
        }
    }

    [Fact]
    public void Add_LargeNumberOfTokens_ShouldHandleGracefully()
    {
        var list = new ArenaList<Token>(_arena, initialCapacity: 4);
        const int total = 10_000;

        for (int i = 0; i < total; i++)
        {
            list.Add(new Token(TokenKind.Number, i, 1));
        }

        var span = list.AsSpan();
        Assert.Equal(total, span.Length);
        Assert.Equal(0, span[0].Start);
        Assert.Equal(total - 1, span[^1].Start);
    }

    public void Dispose() => _arena.Dispose();
}
