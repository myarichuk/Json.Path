using JsonPath.Parser;
using JsonPath.Parser.Helpers;
using JsonPath.Parser.Lexer;
using Xunit;

namespace Json.Path.Tests.Helpers;

public class ArenaListTests : IDisposable
{
    // small arena size to force growth frequently
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

        // trigger growth
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

    [Fact]
    public void CopyingList_ShouldShareHeader()
    {
        var list1 = new ArenaList<Token>(_arena, initialCapacity: 2);
        list1.Add(new Token(TokenKind.Dot, 1, 1));

        var list2 = list1;
        list2.Add(new Token(TokenKind.Identifier, 2, 1));

        // both should see shared state
        var span1 = list1.AsSpan();
        var span2 = list2.AsSpan();

        Assert.Equal(2, span1.Length);
        Assert.Equal(2, span2.Length);
        Assert.Equal(TokenKind.Dot, span1[0].Kind);
        Assert.Equal(TokenKind.Identifier, span1[1].Kind);
    }

    [Fact]
    public void Growth_ShouldPreserveSharedHeader()
    {
        var list1 = new ArenaList<Token>(_arena, initialCapacity: 1);
        var list2 = list1;

        for (int i = 0; i < 10; i++)
        {
            list1.Add(new Token(TokenKind.Number, i, 1));
        }

        // Growth should update shared header's Data pointer
        Assert.Equal(10, list2.Length);
        var span = list2.AsSpan();
        Assert.Equal(9, span[^1].Start);
    }

    private struct Container
    {
        public ArenaList<Token> List;
    }

    [Fact]
    public void ListInsideStruct_ShouldMaintainSharedState()
    {
        var container1 = new Container { List = new ArenaList<Token>(_arena, 2) };
        container1.List.Add(new Token(TokenKind.Root, 0, 1));

        var container2 = container1; // copy struct

        container2.List.Add(new Token(TokenKind.Current, 1, 1));

        var span = container1.List.AsSpan();
        Assert.Equal(2, span.Length);
        Assert.Equal(TokenKind.Root, span[0].Kind);
        Assert.Equal(TokenKind.Current, span[1].Kind);
    }

    [Fact]
    public void Reset_ShouldClearCountButPreserveBuffer()
    {
        var list = new ArenaList<Token>(_arena, 4);
        for (int i = 0; i < 3; i++)
        {
            list.Add(new Token(TokenKind.Number, i, 1));
        }

        list.Reset();

        Assert.True(list.IsEmpty);
        Assert.Equal(0, list.Length);

        // Add again after reset
        list.Add(new Token(TokenKind.Identifier, 42, 1));
        var span = list.AsSpan();
        Assert.Equal(1, span.Length);
        Assert.Equal(42, span[0].Start);
    }

    public void Dispose() => _arena.Dispose();
}
