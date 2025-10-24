using System.Runtime.InteropServices;
using JsonPath.Parser.Lexer;

namespace JsonPath.Parser;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Token(TokenKind kind, int start, int length, int line, int col)
{
    public readonly TokenKind Kind = kind;
    public readonly int Start = start;
    public readonly int Length = length;
    public readonly int Line = line;
    public readonly int Column = col;

    public ReadOnlySpan<char> Slice(ReadOnlySpan<char> input) =>
        input.Slice(Start, Length);
}