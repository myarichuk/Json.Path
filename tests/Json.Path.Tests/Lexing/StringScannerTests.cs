using JsonPath.Parser.Allocators;
using JsonPath.Parser.Diagnostics;
using JsonPath.Parser.Helpers;
using JsonPath.Parser.Lexer;
using Xunit;

namespace Json.Path.Tests.Lexing;

public class StringScannerTests
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

    [Theory]
    [InlineData("\"")]
    [InlineData("'")]
    [InlineData("\"foobar")]
    [InlineData("'foobar")]
    [InlineData("foobar'")]
    [InlineData("'foobar\\\"")]
    [InlineData("\"foobar'")]
    [InlineData("AaA\"foobar\"")]
    [InlineData("'fo\"")]
    public void ShouldNotMatch_MalformedStrings(string input)
    {
        CreateContext(input, out var ctx, out var allocator, out var errors);
        try
        {
            var subscanner = new StringScanner();

            var result = subscanner.TryScan(ref ctx, allocator, errors, out _);
            Assert.False(result);
            if (input.StartsWith("\"") || input.StartsWith("'"))
            {
                Assert.False(errors.IsEmpty);
            }
            else
            {
                Assert.True(errors.IsEmpty);
            }
            Assert.Equal(0, ctx.Position); // do not consume if no match!
        }
        finally
        {
            allocator.Dispose();
        }
    }

    [Theory]
    [InlineData("'test'", 1, 4)]
    [InlineData("'a'", 1, 1)] // edge case
    [InlineData("'foobar'", 1, 6)]
    [InlineData("\"foobar\"", 1, 6)]
    [InlineData("\"a\"", 1, 1)] // edge case
    [InlineData("\"foo\\\"bar\"", 1, 8)] // include escaped character
    [InlineData("''", 1, 0)]
    [InlineData("\"\"", 1, 0)]
    public void CanMatch_ProperStrings(
        string input,
        int expectedTokenStart,
        int expectedLength)
    {
        CreateContext(input, out var ctx, out var allocator, out var errors);
        try
        {
            var subscanner = new StringScanner();

            var result = subscanner.TryScan(ref ctx, allocator, errors, out var token);
            Assert.True(result);
            Assert.True(errors.IsEmpty);
            Assert.Equal(expectedTokenStart, token.Start);
            Assert.Equal(expectedLength, token.Length);
        }
        finally
        {
            allocator.Dispose();
        }
    }

    [Theory]
    [InlineData("foo123\"string_content\"", 14)]
    [InlineData("foo123\"a\\\"b\"", 4)]
    [InlineData("foo123\"a__b\"", 4)]
    [InlineData("foo123\"a\a\bb\"", 4)]
    [InlineData("foo1$['a\\'b']", 4)]
    public void CanMatch_Not_FromStart(string input, int expecteLength)
    {
        CreateContext(input, out var ctx, out var allocator, out var errors);
        try
        {
            ctx.Consume(6); // simulate mid-lexing
            var subscanner = new StringScanner();

            var result = subscanner.TryScan(ref ctx, allocator, errors, out var token);
            Assert.True(result);
            Assert.True(errors.IsEmpty);
            Assert.Equal(7, token.Start);
            Assert.Equal(expecteLength, token.Length);
        }
        finally
        {
            allocator.Dispose();
        }
    }

    [Fact]
    public void Emits_Error_For_Unterminated_String()
    {
        const string input = "'foo";
        CreateContext(input, out var ctx, out var allocator, out var errors);
        try
        {
            var subscanner = new StringScanner();

            var result = subscanner.TryScan(ref ctx, allocator, errors, out _);

            Assert.False(result);
            Assert.False(errors.IsEmpty);

            var error = errors.AsSpan()[0];
            Assert.Equal("string", error.Code.ToString());
            Assert.Equal(DiagnosticPhase.Lexer, error.Phase);
            Assert.Equal(0, error.Span.Start);
            Assert.Equal(input.Length, error.Span.Length);
        }
        finally
        {
            allocator.Dispose();
        }
    }

    [Theory]
    [InlineData("'a\\\'b'", 1, 4)]
    [InlineData("\"a'\"", 1, 2)]
    [InlineData("'a\"b'", 1, 3)]
    public void Supports_Escapes_And_Mixed_Quotes(
        string input,
        int expectedStart,
        int expectedLength)
    {
        CreateContext(input, out var ctx, out var allocator, out var errors);
        try
        {
            var subscanner = new StringScanner();

            var result = subscanner.TryScan(ref ctx, allocator, errors, out var token);

            Assert.True(result);
            Assert.True(errors.IsEmpty);
            Assert.Equal(expectedStart, token.Start);
            Assert.Equal(expectedLength, token.Length);
        }
        finally
        {
            allocator.Dispose();
        }
    }
}