namespace JsonPath.Parser.Lexer;

public static class LexerExtensions
{
    public static bool TryScanForLiteral(this ScanContext ctx, int offset, ReadOnlySpan<char> literal, out Token token)
    {
        token = default;

        var start = ctx.Position + offset;
        if (start < 0 || start + literal.Length > ctx.Input.Length)
        {
            return false;
        }

        var slice = ctx.Input.Slice(start, literal.Length);
        if (!slice.SequenceEqual(literal))
        {
            return false;
        }

        token = new Token(
            TokenKind.Eof,
            start,
            literal.Length);
        return true;
    }

    public static bool TryPeekUntil(this ScanContext ctx, int offset, ReadOnlySpan<char> until, out Token token)
    {
        token = default;

        if (until.Length == 0)
        {
            return false;
        }

        var start = ctx.Position + offset;
        var input = ctx.Input;

        if (start >= input.Length)
        {
            return false;
        }

        int matchIndex = -1;

        if (until.Length == 1)
        {
            var relevantInputSlice = input[start..];

            var index = relevantInputSlice.IndexOf(until[0]);
            if (index >= 0)
            {
                matchIndex = start + index;
            }
        }
        else
        {
            var limit = input.Length - until.Length;
            for (var i = start; i <= limit; i++)
            {
                if (input[i] == until[0] &&
                    input.Slice(i, until.Length)
                         .SequenceEqual(until))
                {
                    matchIndex = i;
                    break;
                }
            }
        }

        if (matchIndex < 0)
        {
            return false;
        }

        var tokenLength = matchIndex - start;
        token = new Token(
            TokenKind.Eof,
            start,
            tokenLength);

        return true;
    }
}