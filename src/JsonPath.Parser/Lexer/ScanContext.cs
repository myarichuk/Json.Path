namespace JsonPath.Parser.Lexer;

public readonly struct ScanProjection
{
    public int Position { get; init; }

    public int Line { get; init; }

    public int Column { get; init; }
}

public ref struct ScanContext(ReadOnlySpan<char> input)
{
    public readonly ReadOnlySpan<char> Input = input;
    public int Position;
    public int Line;
    public int Column;

    public char Current => 
        Position < Input.Length ? Input[Position] : '\0';

    public int Remaining => Input.Length - Position;

    public char Peek(int offset = 1) =>
        Position + offset < Input.Length ? Input[Position + offset] : '\0';

    public void Consume(int count = 1)
    {
        for (int i = 0; i < count && Position < Input.Length; i++)
        {
            var ch = Input[Position++];
            switch (ch)
            {
                case '\r':
                    if (Position < Input.Length && Input[Position] == '\n')
                    {
                        Position++;
                    }

                    Line++;
                    Column = 1;
                    break;
                case '\n':
                case '\u2028': // line separator
                case '\u2029': // paragraph separator
                    Line++;
                    Column = 1;
                    break;
                default:
                    Column++;
                    break;
            }
        }
    }

    public ReadOnlySpan<char> Slice(int start) => Input[start..Position];

    public ScanProjection Project(int absolutePosition)
    {
        int pos = 0;
        int line = 1;
        int column = 1;

        for (; pos < absolutePosition && pos < Input.Length; pos++)
        {
            var ch = Input[pos];
            switch (ch)
            {
                case '\r':
                    if (pos + 1 < Input.Length && Input[pos + 1] == '\n')
                        pos++;
                    line++;
                    column = 1;
                    break;
                case '\n':
                case '\u2028':
                case '\u2029':
                    line++;
                    column = 1;
                    break;
                default:
                    column++;
                    break;
            }
        }

        return new ScanProjection
        {
            Position = absolutePosition,
            Line = line,
            Column = column,
        };
    }
}