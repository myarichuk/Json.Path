using System.Runtime.CompilerServices;

namespace JsonPath.Parser.Lexer;

public class StringScanner: ISubScanner
{
    private const string EscapedDoubleQuote = "\\\"";
    private const string EscapedSingleQuote = "\\'";
    private static readonly string[] EscapeCharacters = [EscapedDoubleQuote];

    public bool TryScan(ref ScanContext ctx, out Token token)
    {
        token = default;

        // only one character can't be string!
        if (ctx.RemainingLength <= 1)
        {
            return false;
        }

        // "short-circuit"
        if (ctx.RemainingLength == 2)
        {
            token = new Token(
                TokenKind.String,
                ctx.Position + 1,
                0,
                ctx.Line,
                ctx.Column);

            var hasSingleQuotes = ctx.Current == '\'' &&
                                  ctx.Peek() == '\'';
            var hasDoubleQuotes = ctx.Current == '"' &&
                                  ctx.Peek() == '"';

            ctx.Consume(2);
            return hasSingleQuotes ||
                   hasDoubleQuotes;
        }

        var isSingleQuote = false;
        if (ctx.TryScanForLiteral(0, "'", out _))
        {
            isSingleQuote = true;
        }
        else if (!ctx.TryScanForLiteral(0, "\"", out _))
        {
            return false;
        }

        var relevantInput = ctx.SliceOffset(1);
        var scanned = 0;
        var hasFoundEnd = false;
        do
        {
            if (relevantInput[scanned..].Length >= 2)
            {
                var maybeEscape =
                    relevantInput.Slice(scanned, 2);

                if (maybeEscape.SequenceEqual(EscapedDoubleQuote))
                {
                    scanned += 2;
                    continue;
                }

                if (isSingleQuote && maybeEscape.SequenceEqual(EscapedSingleQuote))
                {
                    scanned += 2;
                    continue;
                }
            }

            var c = relevantInput[scanned];
            if (c == '"' && isSingleQuote && scanned == ctx.Input.Length - 1)
            {
                break;
            }

            if (c == '\'' && !isSingleQuote && scanned == ctx.Input.Length - 1)
            {
                break;
            }

            if ((isSingleQuote && c == '\'') ||
                (!isSingleQuote && c == '"'))
            {
                hasFoundEnd = true;
                scanned++;
                break;
            }

            scanned++;
        }
        while (scanned < relevantInput.Length);

        if (!hasFoundEnd)
        {
            return false;
        }

        token = new Token(
            TokenKind.String,
            ctx.Position + 1,
            scanned - 1,
            ctx.Line,
            ctx.Column);

        ctx.Consume(token.Length + 2);
        return true;
    }
}