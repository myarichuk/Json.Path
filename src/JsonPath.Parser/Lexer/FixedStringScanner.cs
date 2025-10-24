using System.Runtime.CompilerServices;

namespace JsonPath.Parser.Lexer;

public class FixedStringScanner(string literal, TokenKind kind) : ISubScanner
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryScan(ref ScanContext ctx, out Token token)
    {
        if (ctx.Remaining >= literal.Length &&
            ctx.Input.Slice(ctx.Position, literal.Length).SequenceEqual(literal))
        {
            token = new Token(kind, ctx.Position, literal.Length, ctx.Line, ctx.Column);
            ctx.Consume(literal.Length);

            return true;
        }

        token = default;
        return false;
    }

}