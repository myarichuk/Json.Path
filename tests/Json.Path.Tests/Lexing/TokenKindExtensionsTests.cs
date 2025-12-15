using JsonPath.Parser.Lexer;
using Xunit;

namespace Json.Path.Tests.Lexing;

public class TokenKindExtensionsTests
{
    [Theory]
    [InlineData("$", TokenKind.Root)]
    [InlineData("@", TokenKind.Current)]
    [InlineData(".", TokenKind.Dot)]
    [InlineData("..", TokenKind.DotDot)]
    [InlineData("==", TokenKind.Eq)]
    [InlineData("!=", TokenKind.Ne)]
    [InlineData("&&", TokenKind.And)]
    [InlineData("||", TokenKind.Or)]
    [InlineData("true", TokenKind.True)]
    [InlineData("false", TokenKind.False)]
    [InlineData("null", TokenKind.Null)]
    public void TryGetTokenKind_ShouldReturnKind(string token, TokenKind expectedKind)
    {
        var result = token.TryGetTokenKind(out var kind);

        Assert.True(result);
        Assert.Equal(expectedKind, kind);
    }

    [Theory]
    [InlineData(TokenKind.Root, "$")]
    [InlineData(TokenKind.Current, "@")]
    [InlineData(TokenKind.Dot, ".")]
    [InlineData(TokenKind.DotDot, "..")]
    [InlineData(TokenKind.And, "&&")]
    [InlineData(TokenKind.Or, "||")]
    [InlineData(TokenKind.True, "true")]
    public void GetTokenString_ShouldReturnString(TokenKind kind, string expected)
    {
        var tokenString = kind.GetTokenString();

        Assert.Equal(expected, tokenString);
    }

    [Fact]
    public void TryGetTokenKind_ShouldReturnFalseForUnknownToken()
    {
        var result = "??".TryGetTokenKind(out var kind);

        Assert.False(result);
        Assert.Equal(TokenKind.Eof, kind);
    }

    [Fact]
    public void GetTokenString_ReturnsNullForUnknownKind()
    {
        var tokenString = TokenKind.Eof.GetTokenString();

        Assert.Null(tokenString);
    }

    [Fact]
    public void TokenLookup_ShouldContainMappedTokens()
    {
        var expectedTokens = typeof(TokenKind)
            .GetFields()
            .Select(f => f.GetCustomAttributes(typeof(TokenStringAttribute), false).FirstOrDefault() as TokenStringAttribute)
            .Where(a => a != null)
            .Select(a => a!.Token)
            .ToList();

        var lookup = TokenKindExtensions.TokenLookup;

        foreach (var token in expectedTokens)
        {
            Assert.Contains(token, lookup.Keys);
        }
    }

    [Fact]
    public void TokenLookup_ShouldMapBothWays()
    {
        foreach (var kvp in TokenKindExtensions.TokenLookup)
        {
            var token = kvp.Key;
            var kind = kvp.Value;

            Assert.True(token.TryGetTokenKind(out var reverseKind));
            Assert.Equal(kind, reverseKind);

            Assert.Equal(token, kind.GetTokenString());
        }
    }
}