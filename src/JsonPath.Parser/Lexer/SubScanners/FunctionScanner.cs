namespace JsonPath.Parser.Lexer;

public class FunctionScanner : ISubScanner
{
    public bool TryScan(ref ScanContext ctx, out Token token)
    {
        token = default;
        var identifierScanner = new IdentifierScanner();
        var lParenScanner = new TokenScanner(
            TokenKind.LParen.GetTokenString()!,
            TokenKind.LParen);

        var rParenScanner = new TokenScanner(
            TokenKind.RParen.GetTokenString()!,
            TokenKind.RParen);

        var commaScanner = new TokenScanner(
            TokenKind.Comma.GetTokenString()!,
            TokenKind.Comma);

        if (!identifierScanner.TryScan(ref ctx, out var functionName))
        {
            return false;
        }

        if (!lParenScanner.TryScan(ref ctx, out var lParenToken))
        {
            return false;
        }

        // we start scanning params from this location
        var currentOffset = lParenToken.Start + 1;

        while (currentOffset < ctx.RemainingLength)
        {
            
        }
        
        return false;
    }
}