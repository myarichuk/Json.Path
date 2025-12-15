using System.Runtime.CompilerServices;
using JsonPath.Parser.Allocators;
using JsonPath.Parser.Diagnostics;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser.Lexer;

/// <summary>
/// Handles quoted string literals, supporting escaped delimiters.
/// </summary>
public class StringScanner: ISubScanner
{
    private const string EscapedDoubleQuote = "\\\"";
    private const string EscapedSingleQuote = "\\'";
    private static readonly string[] EscapeCharacters = [EscapedDoubleQuote, EscapedSingleQuote];
    private static readonly string[] DoubleQuoteEscapes = [EscapedDoubleQuote];

    /// <inheritdoc />
    public bool TryScan(
        ref ScanContext ctx,
        ArenaAllocator allocator,
        ArenaList<JsonPathError> errors,
        out Token token)
    {
        token = default;

        // only one character can't be string!
        if (ctx.RemainingLength <= 1)
        {
            if (ctx.Current == '\'' || ctx.Current == '"')
            {
                errors.Add(new JsonPathError(
                    DiagnosticPhase.Lexer,
                    "string",
                    "Unterminated string literal",
                    new SourceSpan(ctx.Position, ctx.RemainingLength),
                    allocator));
            }
            return false;
        }

        // "short-circuit"
        if (ctx.RemainingLength == 2)
        {
            token = new Token(
                TokenKind.String,
                ctx.Position + 1,
                0);

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
            var handledEscape = false;
            if (relevantInput[scanned..].Length >= 2)
            {
                var maybeEscape = relevantInput.Slice(scanned, 2);

                var applicableEscapes = isSingleQuote
                    ? EscapeCharacters
                    : DoubleQuoteEscapes;

                foreach (var escape in applicableEscapes)
                {
                    if (maybeEscape.SequenceEqual(escape))
                    {
                        scanned += 2;
                        handledEscape = true;
                        break;
                    }
                }
            }

            if (handledEscape)
            {
                continue;
            }

            var c = relevantInput[scanned];
            if (c == '"' && isSingleQuote && scanned == relevantInput.Length - 1)
            {
                break;
            }

            if (c == '\'' && !isSingleQuote && scanned == relevantInput.Length - 1)
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
            errors.Add(new JsonPathError(
                DiagnosticPhase.Lexer,
                "string",
                "Unterminated string literal",
                new SourceSpan(ctx.Position, ctx.RemainingLength),
                allocator));
            return false;
        }

        token = new Token(
            TokenKind.String,
            ctx.Position + 1,
            scanned - 1);

        ctx.Consume(token.Length + 2);
        return true;
    }
}
