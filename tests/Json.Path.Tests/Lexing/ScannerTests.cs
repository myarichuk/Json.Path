using JsonPath.Parser;
using JsonPath.Parser.Allocators;
using JsonPath.Parser.Diagnostics;
using JsonPath.Parser.Lexer;
using Xunit;

namespace Json.Path.Tests.Lexing;

public class ScannerTests
{
    private static readonly SubScannerRepository ScannerRepository = new();

    [Fact]
    public void Can_Handle_Simple_Lexing()
    {
        var input = "$['foobar']";

        TokenKind[] expected =
        [
            TokenKind.Root,
            TokenKind.LBracket,
            TokenKind.String,
            TokenKind.RBracket
        ];
        using var allocator = new ArenaAllocator();
        var scanner = new Scanner(allocator, ScannerRepository);

        Assert.True(scanner.TryScan(input, out var tokens, out var errors));
        var tokenCollection = tokens.AsSpan();
        Assert.True(errors.IsEmpty, "errors are not expected!");
        Assert.False(tokens.IsEmpty, "expected successful tokenization");
        Assert.Equal(expected.Length, tokenCollection.Length);

        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], tokenCollection[i].Kind);
            Assert.NotEqual(0, tokenCollection[i].Length);
        }
    }

    [Fact]
    public void Produces_Comprehensive_Token_Stream_For_Query()
    {
        const string input = "$..book[?(@.price>=10&&@.category=='fiction')].title";
        TokenKind[] expected =
        [
            TokenKind.Root,
            TokenKind.DotDot,
            TokenKind.Identifier,
            TokenKind.LBracket,
            TokenKind.Question,
            TokenKind.LParen,
            TokenKind.Current,
            TokenKind.Dot,
            TokenKind.Identifier,
            TokenKind.Ge,
            TokenKind.Number,
            TokenKind.And,
            TokenKind.Current,
            TokenKind.Dot,
            TokenKind.Identifier,
            TokenKind.Eq,
            TokenKind.String,
            TokenKind.RParen,
            TokenKind.RBracket,
            TokenKind.Dot,
            TokenKind.Identifier
        ];

        using var allocator = new ArenaAllocator();
        var scanner = new Scanner(allocator, ScannerRepository);

        var success = scanner.TryScan(input, out var tokens, out var errors);
        Assert.True(success);
        Assert.True(errors.IsEmpty);

        var tokenSpan = tokens.AsSpan();
        Assert.Equal(expected.Length, tokenSpan.Length);

        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], tokenSpan[i].Kind);
        }

        var foundString = false;
        var stringToken = default(Token);
        foreach (var token in tokenSpan)
        {
            if (token.Kind == TokenKind.String)
            {
                stringToken = token;
                foundString = true;
                break;
            }
        }

        Assert.True(foundString, "Expected to locate string literal token");
        Assert.Equal("fiction", stringToken.Slice(input));
    }

    [Fact]
    public void Ignores_Whitespace_Between_Tokens()
    {
        const string input = "  $  ..  book [ ? ( @ . price >= 10 && @ . category == 'fiction' ) ]  .  title  ";
        TokenKind[] expected =
        [
            TokenKind.Root,
            TokenKind.DotDot,
            TokenKind.Identifier,
            TokenKind.LBracket,
            TokenKind.Question,
            TokenKind.LParen,
            TokenKind.Current,
            TokenKind.Dot,
            TokenKind.Identifier,
            TokenKind.Ge,
            TokenKind.Number,
            TokenKind.And,
            TokenKind.Current,
            TokenKind.Dot,
            TokenKind.Identifier,
            TokenKind.Eq,
            TokenKind.String,
            TokenKind.RParen,
            TokenKind.RBracket,
            TokenKind.Dot,
            TokenKind.Identifier
        ];

        using var allocator = new ArenaAllocator();
        var scanner = new Scanner(allocator, ScannerRepository);

        var success = scanner.TryScan(input, out var tokens, out var errors);

        Assert.True(success);
        Assert.True(errors.IsEmpty);

        var tokenSpan = tokens.AsSpan();
        Assert.Equal(expected.Length, tokenSpan.Length);

        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], tokenSpan[i].Kind);
        }
    }

    [Fact]
    public void Stops_And_Reports_When_Token_Unrecognized()
    {
        const string input = "$..book#";

        using var allocator = new ArenaAllocator();
        var scanner = new Scanner(allocator, ScannerRepository);

        var success = scanner.TryScan(input, out var tokens, out var errors);

        Assert.False(success);
        Assert.False(errors.IsEmpty);

        var error = errors.AsSpan()[0];
        Assert.Equal("scanner", error.Code.ToString());
        Assert.Equal(DiagnosticPhase.Lexer, error.Phase);
        Assert.Equal(input.Length - 1, error.Span.Start);

        var tokenSpan = tokens.AsSpan();
        Assert.Equal(3, tokenSpan.Length);
        Assert.Equal(TokenKind.Identifier, tokenSpan[^1].Kind);
    }

    [Fact]
    public void Prefers_Longest_Matching_Token()
    {
        const string input = "$..a";

        using var allocator = new ArenaAllocator();
        var scanner = new Scanner(allocator, ScannerRepository);

        var success = scanner.TryScan(input, out var tokens, out var errors);

        Assert.True(success);
        Assert.True(errors.IsEmpty);

        var tokenSpan = tokens.AsSpan();
        Assert.Equal(3, tokenSpan.Length);
        Assert.Equal(TokenKind.Root, tokenSpan[0].Kind);
        Assert.Equal(TokenKind.DotDot, tokenSpan[1].Kind);
        Assert.Equal(TokenKind.Identifier, tokenSpan[2].Kind);
    }

    [Fact]
    public void Recognizes_Literal_Keywords()
    {
        const string input = "$[true,false,null]";

        using var allocator = new ArenaAllocator();
        var scanner = new Scanner(allocator, ScannerRepository);

        var success = scanner.TryScan(input, out var tokens, out var errors);

        Assert.True(success);
        Assert.True(errors.IsEmpty);

        var tokenSpan = tokens.AsSpan();
        TokenKind[] expected =
        [
            TokenKind.Root,
            TokenKind.LBracket,
            TokenKind.True,
            TokenKind.Comma,
            TokenKind.False,
            TokenKind.Comma,
            TokenKind.Null,
            TokenKind.RBracket,
        ];

        Assert.Equal(expected.Length, tokenSpan.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], tokenSpan[i].Kind);
        }
    }
}
