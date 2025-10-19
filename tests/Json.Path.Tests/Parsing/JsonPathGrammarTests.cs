using Json.Path.Parsing;
using Json.Path.Tests.Infrastructure;
using Xunit;

namespace Json.Path.Tests.Parsing;

public class JsonPathGrammarTests(AntlrFixture<JsonPathLexer, JsonPathParser> fixture)
    : IClassFixture<AntlrFixture<JsonPathLexer, JsonPathParser>>
{
    private JsonPathParser.ExpressionContext Parse(string input, out JsonPathParser parser)
    {
        parser = fixture.CreateParser(input);
        return parser.expression();
    }

    // VALID: root and property access
    [Theory]
    [InlineData("$")]
    [InlineData("$.store")]
    [InlineData("$['store']")]
    [InlineData("$['store'].book")]
    [InlineData("$.store['book']")]
    public void ParsesSimpleRootAndPropertyAccess(string input)
    {
        var context = Parse(input, out var parser);
        Assert.NotNull(context);
        Assert.Equal(0, parser.NumberOfSyntaxErrors);
    }

    // VALID: array indexers
    [Theory]
    [InlineData("$.book[0]")]
    [InlineData("$.book[123]")]
    [InlineData("$['book'][0]")]
    [InlineData("$[0]")]
    [InlineData("$[0]['book']")]
    [InlineData("$[0].book")]
    [InlineData("$[*]")]
    [InlineData("$[*]['book']")]
    [InlineData("$[*].book")]
    public void ParsesArrayIndexers(string input)
    {
        var context = Parse(input, out var parser);
        Assert.NotNull(context);
        Assert.Equal(0, parser.NumberOfSyntaxErrors);
    }

    // VALID: wildcards and mixed chaining
    [Theory]
    [InlineData("$.book.*")]
    [InlineData("$.book.*[0]")]
    [InlineData("$.book.*[0]['a']")]
    [InlineData("$.*")]
    [InlineData("$.*[*]")]
    [InlineData("$.*.*")]
    [InlineData("$..*.*")]
    [InlineData("$.*['a']")]
    public void ParsesWildcardSegments(string input)
    {
        var context = Parse(input, out var parser);
        Assert.NotNull(context);
        Assert.Equal(0, parser.NumberOfSyntaxErrors);
    }

    // VALID: recursive descent
    [Theory]
    [InlineData("$..author")]
    [InlineData("$..*")]
    [InlineData("$..book[0]")]
    [InlineData("$.book..author")]
    [InlineData("$..book..author")]
    public void ParsesRecursiveDescent(string input)
    {
        var context = Parse(input, out var parser);
        Assert.NotNull(context);
        Assert.Equal(0, parser.NumberOfSyntaxErrors);
    }

    // VALID: bracket-notation properties
    [Theory]
    [InlineData("$.book['title']")]
    [InlineData("$..book['title']")]
    [InlineData("$.book['title'][0]")]
    [InlineData("$['book']['title']")]
    [InlineData("$['book'][0]['title']")]
    [InlineData("$['weird\\'name']")]
    [InlineData("$['escaped\\\"quote']")]
    [InlineData("$['unicode\\u0041']")]
    [InlineData("$['']")]
    public void ParsesBracketNotationProperties(string input)
    {
        var context = Parse(input, out var parser);
        Assert.NotNull(context);
        Assert.Equal(0, parser.NumberOfSyntaxErrors);
    }

    // VALID: deeply chained segments
    [Theory]
    [InlineData("$.store.book[0]['title']")]
    [InlineData("$.store.book[0].author")]
    [InlineData("$['store'].book[0]['title']")]
    [InlineData("$.a.b.c.d.e.f.g.h.i.j.k")]
    [InlineData("$['a']['b']['c']['d']['e']['f']['g']")]
    public void ParsesChainedSegments(string input)
    {
        var context = Parse(input, out var parser);
        Assert.NotNull(context);
        Assert.Equal(0, parser.NumberOfSyntaxErrors);
    }

    // INVALID: general syntax violations
    [Theory]

    [InlineData("$.book.*[0].['a']")]
    [InlineData("$.store.['book']")]
    [InlineData("$store")]
    [InlineData("$store.book[0].author")]

    // invalid index content
    [InlineData("$.book[notanumber]")]
    [InlineData("$.book[01]")] // leading zero violates RFC
    [InlineData("$[notanumber]")]

    // unterminated or malformed strings
    [InlineData("$['unclosed]")]
    [InlineData("$['unterminated\\u00']")]

    // stray commas or empty selectors
    [InlineData("$['a',]")]
    [InlineData("$[]")]
    [InlineData("$.book[]")]

    // bad recursive descent usage
    [InlineData("$.book..")]
    [InlineData("$..")]
    [InlineData("$..book..")]
    [InlineData("$..*['a']")] // recursive segment must have one selector
    [InlineData("$...book")] // triple dot nonsense

    // slices and filters (TODO: don't forget to implement!)
    [InlineData("$.book[1:3]")]
    [InlineData("$.book[:3]")]
    [InlineData("$.book[::2]")]
    [InlineData("$.book[1:3:2]")]
    [InlineData("$.book[?()]")]
    [InlineData("$.book[??@.price]")]
    public void RejectsInvalidSyntax(string input)
    {
        var parser = fixture.CreateParser(input);
        parser.expression();
        Assert.NotEqual(0, parser.NumberOfSyntaxErrors);
    }
}
