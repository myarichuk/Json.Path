using System;
using System.Collections.Generic;
using System.Linq;

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

        [TokenString("..")]
        DotDot,

        [TokenString(".")]
        Dot,

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

        [TokenString("<=")]
        Le,

        [TokenString("<")]
        Lt,

        [TokenString(">=")]
        Ge,

        [TokenString(">")]
        Gt,

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
}
