using System.Runtime.CompilerServices;

using JsonPath.Parser.Allocators;
using JsonPath.Parser.Diagnostics;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser.Lexer;

/// <summary>
/// Matches fixed literal tokens (operators, punctuation, keywords).
/// </summary>
public class TokenScanner(string literal, TokenKind kind) : ISubScanner
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// <inheritdoc />
    public bool TryScan(
        ref ScanContext ctx,
        ArenaAllocator allocator,
        ArenaList<JsonPathError> errors,
        out Token token)
    {
        if (ctx.RemainingLength >= literal.Length &&
            ctx.Input.Slice(ctx.Position, literal.Length).SequenceEqual(literal))
        {
            token = new Token(kind, ctx.Position, literal.Length);
            ctx.Consume(literal.Length);

            return true;
        }

        token = default;
        return false;
    }
}
