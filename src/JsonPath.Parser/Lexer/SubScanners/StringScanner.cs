using System.Runtime.CompilerServices;

namespace JsonPath.Parser.Lexer;

public class StringScanner: ISubScanner
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryScan(ref ScanContext ctx, out Token token)
    {
        token = default;

        var isSingleQuote = false;

        if (ctx.TryPeekForLiteral(0, "'", out _))
        {
            isSingleQuote = true;
        }
        else if (!ctx.TryPeekForLiteral(0, "\"", out _))
        {
            return false;
        }

        // TODO: handle string escaping
        if (!ctx.TryPeekUntil(1, isSingleQuote ? "'" : "\"", out var stringToken))
        {
            return false;
        }

        // note: don't include the quotes as they are not a part of the string
        token = new Token(TokenKind.String, ctx.Position + 1, stringToken.Length - 1, ctx.Line, ctx.Column);
        ctx.Consume(token.Length);
        return true;
    }
}