using System.Runtime.CompilerServices;

namespace JsonPath.Parser.Lexer;

/// <summary>
/// Parses integer and floating point literals, including optional exponent parts.
/// </summary>
public class NumberScanner: ISubScanner
{
    /// <inheritdoc />
    public bool TryScan(ref ScanContext ctx, out Token token)
    {
        token = default;

        var remaining = ctx.RemainingInput;
        var scanned = 0;
        var hadSeenDecimalPoint = false;
        bool hasDigits = false;

        if (ctx.RemainingLength == 0 ||
            (remaining[0] == '.' &&
            (remaining.Length == 1 || !char.IsDigit(remaining[1]))))
        {
            return false;
        }

        while (scanned < remaining.Length)
        {
            var @char = remaining[scanned];
            if (scanned == 0 &&
                @char == '-' &&
                scanned + 1 < remaining.Length &&
                char.IsDigit(remaining[scanned + 1]))
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
            if (IsAsciiDigit(@char))
            {
                scanned++;
                hasDigits = true;
                continue;
            }

            // exponential segment
            if (scanned < remaining.Length && (remaining[scanned] == 'e' || remaining[scanned] == 'E'))
            {
                int expStart = scanned;
                scanned++; // consume 'e' or 'E'

                if (scanned < remaining.Length && IsSign(remaining[scanned]))
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

        // just in case
        if (hadSeenDecimalPoint && !hasDigits)
        {
            return false;
        }

        if (scanned > 0 && hasDigits)
        {
            token = new Token(TokenKind.Number, ctx.Position, scanned);
            ctx.Consume(scanned);
            return true;
        }

        return false;
    }
    
    static bool IsSign(char c) => c is '+' or '-';
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsAsciiDigit(char c) => (uint)(c - '0') <= 9;

}
