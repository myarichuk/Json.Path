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

    public int RemainingLength => Input.Length - Position;

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

    public ReadOnlySpan<char> RemainingInput => Input[Position..];

    public ReadOnlySpan<char> SliceOffset(int offset) => Input[(Position + offset)..];

    public ReadOnlySpan<char> SliceFrom(in Token token) => Input.Slice(token.Start, token.Length);
    
    public ScanProjection Project(int absolutePosition)
    {
        int line = 1;
        int column = 1;

        int pos = 0;
        while (pos < absolutePosition && pos < Input.Length)
        {
            var ch = Input[pos++];
            if (ch == '\r')
            {
                if (pos < Input.Length && Input[pos] == '\n')
                    pos++;
                line++;
                column = 1;
            }
            else if (ch == '\n' || ch == '\u2028' || ch == '\u2029')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
        }

        column = Math.Max(1, column - 1);


        return new ScanProjection
        {
            Position = absolutePosition,
            Line = line,
            Column = column,
        };
    }
}