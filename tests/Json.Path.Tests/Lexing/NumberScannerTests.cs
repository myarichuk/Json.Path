using JsonPath.Parser;
using JsonPath.Parser.Allocators;
using JsonPath.Parser.Diagnostics;
using JsonPath.Parser.Helpers;
using JsonPath.Parser.Lexer;
using Xunit;

namespace Json.Path.Tests.Lexing;

public class NumberScannerTests
{
    private static bool TryScan(string input, out Token token)
    {
        var ctx = new ScanContext(input.AsSpan());
        using var allocator = new ArenaAllocator();
        var errors = new ArenaList<JsonPathError>(allocator);
        var scanner = new NumberScanner();
        var result = scanner.TryScan(ref ctx, allocator, errors, out token);
        Assert.True(errors.IsEmpty);
        return result;
    }

    private static bool TryScan(string input, out Token token, out int consumed)
    {
        var ctx = new ScanContext(input.AsSpan());
        using var allocator = new ArenaAllocator();
        var errors = new ArenaList<JsonPathError>(allocator);
        var scanner = new NumberScanner();
        var result = scanner.TryScan(ref ctx, allocator, errors, out token);
        consumed = ctx.Position;
        Assert.True(errors.IsEmpty);
        return result;
    }

    [Theory]
    [InlineData("123", 3)]
    [InlineData("0", 1)]
    [InlineData("42xyz", 2)]
    [InlineData("3.14", 4)]
    [InlineData("10.0.5", 4)] // stops before second '.'
    public void Should_Correctly_Scan_Numbers(string input, int expectedLength)
    {
        var success = TryScan(input, out var token);

        Assert.True(success);
        Assert.Equal(TokenKind.Number, token.Kind);
        Assert.Equal(expectedLength, token.Length);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData(".x")]
    [InlineData("x123")]
    [InlineData("")]
    [InlineData("-notanumber")]
    [InlineData("-")]
    public void Rejects_Invalid_Or_NonNumber(string input)
    {
        var success = TryScan(input, out _);
        Assert.False(success);
    }

    [Fact]
    public void Updates_Context_Position_After_Scan()
    {
        var ctx = new ScanContext("123abc".AsSpan());
        using var allocator = new ArenaAllocator();
        var errors = new ArenaList<JsonPathError>(allocator);
        var scanner = new NumberScanner();

        var success = scanner.TryScan(ref ctx, allocator, errors, out var token);

        Assert.True(success);
        Assert.True(errors.IsEmpty);
        Assert.Equal(3, ctx.Position); // consumed 3 chars
        Assert.Equal(3, token.Length);
    }

    [Fact]
    public void Handles_Float_With_Trailing_Text()
    {
        var ctx = new ScanContext("3.14foo".AsSpan());
        using var allocator = new ArenaAllocator();
        var errors = new ArenaList<JsonPathError>(allocator);
        var scanner = new NumberScanner();

        var success = scanner.TryScan(ref ctx, allocator, errors, out var token);

        Assert.True(success);
        Assert.True(errors.IsEmpty);
        Assert.Equal(4, token.Length);
        Assert.Equal(4, ctx.Position);
    }

    [Theory]
    [InlineData("1e10", 4)]
    [InlineData("1E10", 4)]
    [InlineData("3.14e2", 6)]
    [InlineData("3.14E+2", 7)]
    [InlineData("6.022E23", 8)]
    [InlineData("0e-1", 4)]
    [InlineData("10e0", 4)]
    public void Scans_Valid_Exponentials(string input, int expectedLength)
    {
        var success = TryScan(input, out var token, out var consumed);

        Assert.True(success);
        Assert.Equal(TokenKind.Number, token.Kind);
        Assert.Equal(expectedLength, token.Length);
        Assert.Equal(expectedLength, consumed);
    }

    [Theory]
    [InlineData("1e", 1)] // missing exponent digits
    [InlineData("1E", 1)]
    [InlineData("1e+", 1)] // sign but no digits
    [InlineData("1e-", 1)]
    [InlineData("1e1.5", 3)] // decimal point not allowed in exponent
    [InlineData("3.14e1.5", 6)] // should stop before second '.'
    [InlineData("3.14e--2", 4)] // nonsense signs
    [InlineData("1E++2", 1)]
    [InlineData("1e+E2", 1)]
    public void Rejects_Invalid_Exponentials(string input, int expectedLength)
    {
        var success = TryScan(input, out var token, out var consumed);

        // var materializedtoken =
        //     new ScanContext(input.AsSpan()).SliceFrom(token);
        Assert.True(success);
        Assert.Equal(expectedLength, token.Length);
        Assert.Equal(expectedLength, consumed);
    }

    [Fact]
    public void Stops_At_NonNumeric_After_Exponent()
    {
        var success = TryScan("1e10abc", out var token, out var consumed);

        Assert.True(success);
        Assert.Equal(4, token.Length);
        Assert.Equal(4, consumed);
    }

    [Fact]
    public void Does_Not_Treat_Dot_In_Exponent_As_Part_Of_Number()
    {
        var success = TryScan("1e1.5", out var token, out var consumed);

        Assert.True(success);
        Assert.Equal(3, token.Length); // "1e1"
        Assert.Equal(3, consumed);
    }

    [Theory]
    [InlineData("3.14E+0", 7)]
    [InlineData("3.14E-0", 7)]
    [InlineData("3E+7", 4)]
    public void Handles_Signed_Exponents(string input, int expectedLength)
    {
        var success = TryScan(input, out var token, out var consumed);

        Assert.True(success);
        Assert.Equal(expectedLength, token.Length);
        Assert.Equal(expectedLength, consumed);
    }

    [Theory]
    [InlineData("1e+01xyz", 5)]
    [InlineData("1e-2!", 4)]
    public void Stops_At_NonDigit_After_ExponentDigits(string input, int expectedLength)
    {
        var success = TryScan(input, out var token, out var consumed);

        Assert.True(success);
        Assert.Equal(expectedLength, token.Length);
        Assert.Equal(expectedLength, consumed);
    }
}