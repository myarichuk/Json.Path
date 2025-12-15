using JsonPath.Parser.Allocators;
using JsonPath.Parser.Diagnostics;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser.Lexer;

/// <summary>
/// Recognizes unquoted identifier segments (e.g. property names) in a JsonPath expression.
/// </summary>
public class IdentifierScanner : ISubScanner
{
    /// <inheritdoc />
    public bool TryScan(
        ref ScanContext ctx,
        ArenaAllocator allocator,
        ArenaList<JsonPathError> errors,
        out Token token)
    {
        token = default;

        if (!char.IsLetter(ctx.Current) && ctx.Current != '_')
        {
            return false;
        }

        var scanned = 0;
        var remaining = ctx.RemainingInput.Slice(1);
        var remainingLength = remaining.Length;
        while (scanned < remainingLength)
        {
            var currentChar = remaining[scanned];
            if (!char.IsLetterOrDigit(currentChar) &&
                currentChar != '_')
            {
                break;
            }

            scanned++;
        }

        var tokenLength = scanned + 1;
        token = new Token(TokenKind.Identifier, ctx.Position, tokenLength);
        ctx.Consume(tokenLength);

        return true;
    }
}
