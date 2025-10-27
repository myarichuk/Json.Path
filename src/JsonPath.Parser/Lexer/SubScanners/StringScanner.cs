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

        if (!ctx.TryPeekUntil(1, isSingleQuote ? "'" : "\"", out var stringToken))
        {
            return false;
        }

        token = new Token(TokenKind.String, ctx.Position, stringToken.Length + 1, ctx.Line, ctx.Column);

        return false;
    }
}