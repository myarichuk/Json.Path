using System.Runtime.InteropServices;
using JsonPath.Parser.Lexer;

namespace JsonPath.Parser;

/// <summary>
/// Represents a lexical token identified in the input stream.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Token(TokenKind kind, int start, int length)
{
    public readonly TokenKind Kind = kind;
    public readonly int Start = start;
    public readonly int Length = length;

    public ReadOnlySpan<char> Slice(ReadOnlySpan<char> input) =>
        input.Slice(Start, Length);
}