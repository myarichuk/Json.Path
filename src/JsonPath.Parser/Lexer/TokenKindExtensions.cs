using System.Reflection;

namespace JsonPath.Parser.Lexer;

public static class TokenKindExtensions
{
    private static readonly Dictionary<string, TokenKind> StringToToken;
    private static readonly Dictionary<TokenKind, string> TokenToString;

    static TokenKindExtensions()
    {
        StringToToken = new Dictionary<string, TokenKind>(StringComparer.Ordinal);
        TokenToString = new Dictionary<TokenKind, string>();

        foreach (var field in typeof(TokenKind).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<TokenStringAttribute>();
            if (attr != null && !string.IsNullOrWhiteSpace(attr.Token))
            {
                var kind = (TokenKind)field.GetValue(null)!;
                StringToToken[attr.Token] = kind;
                TokenToString[kind] = attr.Token;
            }
        }
    }

    public static bool TryGetTokenKind(this string token, out TokenKind kind) =>
        StringToToken.TryGetValue(token, out kind);

    public static string? GetTokenString(this TokenKind kind) =>
        TokenToString.TryGetValue(kind, out var token) ? token : null;

    public static IReadOnlyDictionary<string, TokenKind> TokenLookup => StringToToken;
}