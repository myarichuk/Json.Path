using Json.Path.Parsing;
using Json.Path.Tests.Infrastructure;
using Xunit;

namespace Json.Path.Tests.Parsing;

public class JsonPathGrammarTests(AntlrFixture<JsonPathTestLexer, JsonPathTestParser> fixture)
    : IClassFixture<AntlrFixture<JsonPathTestLexer, JsonPathTestParser>>
{
    [Theory]
    [InlineData("identifier")]
    [InlineData("another_identifier123")]
    public void ParsesIdentifierExpressions(string input)
    {
        var parser = fixture.CreateParser(input);
        var context = parser.expression();

        Assert.NotNull(context);
        Assert.Equal(0, parser.NumberOfSyntaxErrors);
    }
}
