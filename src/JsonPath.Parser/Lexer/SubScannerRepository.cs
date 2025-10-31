using System.Collections;

namespace JsonPath.Parser.Lexer;

public class SubScannerRepository: IEnumerable<ISubScanner>
{
    private static readonly List<ISubScanner> TokenScanners;

    static SubScannerRepository()
    {
        TokenScanners = new(TokenKindExtensions.TokenLookup.Count);
        foreach (var (literal, kind) in
                 TokenKindExtensions.TokenLookup
                     .OrderByDescending(
                         x =>
                             x.Key.Length))
        {
            TokenScanners.Add(new TokenScanner(literal, kind));
        }

        TokenScanners.Add(new StringScanner());
        
        //TODO: don't forget to add other token scanners like one for identifiers
    }

    public IEnumerator<ISubScanner> GetEnumerator() =>
        TokenScanners.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        TokenScanners.GetEnumerator();
}