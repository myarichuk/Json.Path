using System;

namespace JsonPath.Parser.Lexer;

public ref struct ScanContext(ReadOnlySpan<char> input)
{
    public readonly ReadOnlySpan<char> Input = input;
    public int Position;

    public char Current =>
        Position < Input.Length ? Input[Position] : '\0';

    public int RemainingLength => Input.Length - Position;

    public char Peek(int offset = 1) =>
        Position + offset < Input.Length ? Input[Position + offset] : '\0';

    public void Consume(int count = 1)
    {
        Position = Math.Min(Input.Length, Position + Math.Max(0, count));
    }

    public ReadOnlySpan<char> RemainingInput => Input[Position..];

    public ReadOnlySpan<char> SliceOffset(int offset) => Input[(Position + offset)..];

    public ReadOnlySpan<char> SliceFrom(in Token token) => Input.Slice(token.Start, token.Length);
}