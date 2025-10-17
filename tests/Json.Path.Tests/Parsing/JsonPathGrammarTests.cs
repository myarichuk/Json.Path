using Json.Path.Parsing;
using Json.Path.Tests.Infrastructure;
using Xunit;

namespace Json.Path.Tests.Parsing;

public class JsonPathGrammarTests : IClassFixture<AntlrFixture<JsonPathTestLexer, JsonPathTestParser>>
{
    private readonly AntlrFixture<JsonPathTestLexer, JsonPathTestParser> _fixture;

    public JsonPathGrammarTests(AntlrFixture<JsonPathTestLexer, JsonPathTestParser> fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData("identifier")]
    [InlineData("another_identifier123")]
    public void ParsesIdentifierExpressions(string input)
    {
        var parser = _fixture.CreateParser(input);
        var context = parser.expression();

        Assert.NotNull(context);
        Assert.Equal(0, parser.NumberOfSyntaxErrors);
    }
}
