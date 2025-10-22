using Antlr4.Runtime.Tree;
using FluentAssertions;
using Json.Path.Parsing;
using Json.Path.Tests.Infrastructure;
using Xunit;

namespace Json.Path.Tests.Parsing;

public class JsonPathGrammarTests(AntlrFixture<JsonPathLexer, JsonPathParser> fixture)
    : IClassFixture<AntlrFixture<JsonPathLexer, JsonPathParser>>
{
    private JsonPathParser.JsonPathContext Parse(string input, out JsonPathParser parser)
    {
        parser = fixture.CreateParser(input);
        return parser.jsonPath();
    }

    // VALID: root and property access
    [Theory]
    [InlineData("$")]
    [InlineData("$.store")]
    [InlineData("$['store']")]
    [InlineData("$.store['book']")]
    [InlineData("$['store'].book")]
    [InlineData("$['store.abc'].book")]
    [InlineData("$.store['book.abc']")]
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
    [InlineData("$..['book']['title']..*")]
    [InlineData("$..[0]")] // looks bit weird but valid according to RFC
    [InlineData("$..book[0]")]
    [InlineData("$.book..author")]
    [InlineData("$..book..author")]
    public void ParsesRecursiveDescent(string input)
    {
        var context = Parse(input, out var parser);
        Assert.NotNull(context);
        Assert.Equal(0, parser.NumberOfSyntaxErrors);
    }

    // VALID: union selectors
    [Theory]
    [InlineData("$['title','author']")]
    [InlineData("$[0,1,2]")]
    [InlineData("$[0:3,5:7]")]
    [InlineData("$['title',2:5]")]
    [InlineData("$[0,'foo',1:3]")]
    [InlineData("$['', 'empty']")]
    [InlineData("$['a', 'b', 'c']")]
    [InlineData("$.store['book','bicycle']")]
    [InlineData("$..book[0,1]")]
    [InlineData("$[-3 , -1 ]")] // TODO: check if negative indices allowed by RFC
    [InlineData("$[0:3, 7]")]
    [InlineData("$.a[1:2,3:4]")] // TODO: check if two or more slices are allowed in a union
    public void ParsesUnionSelectors(string input)
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
    [InlineData("$['[0]']")] // looks weird but it should be valid
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

    // VALID: array slices (RFC 9535 §2.6)
    [Theory]
    [InlineData("$.a[1:3]")]
    [InlineData("$.a[1:]")]
    [InlineData("$.a[:3]")]
    [InlineData("$.a[:]")]
    [InlineData("$.a[::2]")]
    [InlineData("$.a[-3:]")]
    [InlineData("$.a[:-1]")]
    [InlineData("$.a[1:10:2]")]
    [InlineData("$[1:3]")]
    [InlineData("$['a'][1:2]")]
    [InlineData("$..book[1:2]")]
    public void ParsesArraySlices(string input)
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
    [InlineData("$.book[-01]")] // also "negative" leading zero violates RFC
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
    [InlineData("$...book")] // triple dot nonsense

    // INVALID: array slice syntax
    [InlineData("$.a[1:2:3:4]")] // too many colons
    [InlineData("$.a[1::]")] // missing end after first colon
    [InlineData("$.a[1:two]")] // non-numeric literal
    [InlineData("$.a[:1.5]")] // floats not allowed
    [InlineData("$.a[-1:2:0]")] // start cannot be negative
    [InlineData("$.a[1:2:0]")] // step cannot be zero
    [InlineData("$.a[::]")]

    // INVALID: union syntax
    [InlineData("$[,1,2]")] // leading comma
    [InlineData("$[1,2,]")] // trailing comma
    [InlineData("$[1,,2]")] // empty element
    [InlineData("$[1:2,]")] // trailing comma after slice
    [InlineData("$[1 : 2]")] // space around colon
    [InlineData("$[ 1:2 ]")] // space around numbers
    [InlineData("$[1:two]")] // non-numeric
    [InlineData("$[1, @.foo]")] // expression not allowed
    [InlineData("$['a','b', ]")] // whitespace before trailing comma
    [InlineData("$[1:2:3:4]")] // too many colons
    [InlineData("$[1,2 3]")] // missing comma
    [InlineData("$[1, true]")] // literal not allowed
    [InlineData("$['a':2]")] // invalid syntax mixing name and colon
    public void RejectsInvalidSyntax(string input)
    {
        var parser = fixture.CreateParser(input);
        var ctx = parser.jsonPath();
        var ast = Trees.ToStringTree(ctx, parser);
        (parser.NumberOfSyntaxErrors > 0 || fixture.Validator!.Errors.Count > 0)
            .Should().BeTrue("because invalid input should produce either syntax or semantic errors");
    }
}
