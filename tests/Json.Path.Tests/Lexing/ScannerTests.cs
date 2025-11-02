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
}