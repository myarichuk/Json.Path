namespace JsonPath.Parser.Lexer;

public class NumberScanner: ISubScanner
{
    public bool TryScan(ref ScanContext ctx, out Token token)
    {
        token = default;

        var remaining = ctx.RemainingInput;
        var scanned = 0;
        var hadSeenDecimalPoint = false;

        if (ctx.RemainingLength == 0 ||
            (remaining[0] == '.' &&
            (remaining.Length == 1 || !char.IsDigit(remaining[1]))))
        {
            return false;
        }

        while (scanned < remaining.Length)
        {
            var @char = remaining[scanned];
            if (scanned == 0 && @char == '-')
            {
                scanned++;
                continue;
            }

            if (!hadSeenDecimalPoint &&
                @char == '.')
            {
                scanned++;
                hadSeenDecimalPoint = true;
                continue;
            }

            // do not allow Ⅻ or ٣, only ascii numbers
            if (char.IsDigit(@char))
            {
                scanned++;
                continue;
            }

            // exponential segment
            if (scanned < remaining.Length && (remaining[scanned] == 'e' || remaining[scanned] == 'E'))
            {
                int expStart = scanned;
                scanned++; // consume 'e' or 'E'

                if (scanned < remaining.Length &&
                    (remaining[scanned] == '+' ||
                     remaining[scanned] == '-'))
                {
                    scanned++;
                }

                var digitsStart = scanned;

                // must have at least one digit after exponent marker
                while (scanned < remaining.Length && char.IsDigit(remaining[scanned]))
                {
                    scanned++;
                }

                if (scanned == digitsStart)
                {
                    // Roll back if no digits after e/E
                    scanned = expStart;
                }
            }

            break;
        }

        if (scanned > 0)
        {
            token = new Token(TokenKind.Number, ctx.Position, scanned, ctx.Line, ctx.Column);
            ctx.Consume(scanned);
            return true;
        }

        return false;
    }
}