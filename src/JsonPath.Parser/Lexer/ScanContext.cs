namespace JsonPath.Parser.Lexer;

public ref struct ScanContext(ReadOnlySpan<char> input)
{
    public readonly ReadOnlySpan<char> Input = input;
    public int Position;
    public int Line;
    public int Column;

    public char Current => Input[Position];

    public int Remaining => Input.Length - Position;

    public char Peek(int offset = 1) => Input[Position + offset];

    public void Consume(int count = 1) => Position += count;

    public ReadOnlySpan<char> Slice(int start) => Input[start..Position];
}