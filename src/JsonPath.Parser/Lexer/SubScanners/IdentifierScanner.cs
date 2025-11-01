namespace JsonPath.Parser.Lexer;

public class IdentifierScanner: ISubScanner
{
    public bool TryScan(ref ScanContext ctx, out Token token)
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

        return false;
    }
}