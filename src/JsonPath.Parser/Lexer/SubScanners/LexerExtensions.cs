namespace JsonPath.Parser.Lexer;

public static class LexerExtensions
{
    public static bool TryPeekForLiteral(this ScanContext ctx, int offset, ReadOnlySpan<char> literal, out Token token)
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

        var projection = ctx.Project(offset);

        token = new Token(
            TokenKind.Unknown,
            start,
            literal.Length,
            projection.Line,
            projection.Column
        );
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
        if (start >= ctx.Input.Length)
        {
            return false;
        }

        var input = ctx.Input;

        if (until.Length == 1)
        {
            var index = input[start..].IndexOf(until[0]);
            if (index < 0)
            {
                return false;
            }

            var matchPos = start + index;
            var projection = ctx.Project(matchPos - ctx.Position);

            token = new Token(TokenKind.Unknown, matchPos, until.Length, projection.Line, projection.Column);
            return true;
        }

        var limit = input.Length - until.Length;

        for (var index = start; index <= limit; index++)
        {
            if (input[index] == until[0] &&
                input.Slice(index, until.Length).SequenceEqual(until))
            {
                var projection = ctx.Project(index - ctx.Position);
                token = new Token(TokenKind.Unknown, index, until.Length, projection.Line, projection.Column);
                return true;
            }
        }

        return false;
    }
}