using System.Runtime.CompilerServices;

using JsonPath.Parser;

public unsafe ref struct ParserContext(Token* data, int count)
{
    private readonly int _count = count;
    private int _pos = 0;

    public Token Current => data[_pos];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Token Peek(int offset = 1) => data[_pos + offset];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Consume(int count = 1) => _pos += count;

    public int Position => _pos;
}