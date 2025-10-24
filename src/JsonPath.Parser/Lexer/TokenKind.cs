using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonPath.Parser.Lexer
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TokenStringAttribute : Attribute
    {
        public string Token { get; init; }

        public TokenStringAttribute(string token)
        {
            Token = token;
        }
    }

    public enum TokenKind : byte
    {
        Unknown,
        Eof,
        Identifier,
        Number,
        String,

        [TokenString("$")]
        Root,

        [TokenString("@")]
        Current,

        [TokenString(".")]
        Dot,

        [TokenString("..")]
        DotDot,

        [TokenString("[")]
        LBracket,

        [TokenString("]")]
        RBracket,

        [TokenString("(")]
        LParen,

        [TokenString(")")]
        RParen,

        [TokenString(":")]
        Colon,

        [TokenString(",")]
        Comma,

        [TokenString("?")]
        Question,

        [TokenString("*")]
        Star,

        [TokenString("~=")]
        RegexMatch,

        [TokenString("==")]
        Eq,

        [TokenString("!=")]
        Ne,

        [TokenString("<")]
        Lt,

        [TokenString("<=")]
        Le,

        [TokenString(">")]
        Gt,

        [TokenString(">=")]
        Ge,

        [TokenString("&&")]
        And,

        [TokenString("||")]
        Or,

        [TokenString("!")]
        Not,

        [TokenString("+")]
        Add,

        [TokenString("-")]
        Sub,

        [TokenString("/")]
        Div,

        [TokenString("%")]
        Mod,

        [TokenString("true")]
        True,

        [TokenString("false")]
        False,

        [TokenString("null")]
        Null,
    }

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
}
