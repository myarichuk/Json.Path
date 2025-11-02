using System.Collections;

namespace JsonPath.Parser.Lexer;

/// <summary>
/// Provides the ordered set of sub-scanners that the <see cref="Scanner"/> consults.
/// </summary>
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

        // I know, hardcoded is meh but no reason to do fancy reflection
        // (I mean, there is only so many of those :) )
        TokenScanners.Add(new StringScanner());
        TokenScanners.Add(new IdentifierScanner());
        TokenScanners.Add(new NumberScanner());
    }

    public IEnumerator<ISubScanner> GetEnumerator() =>
        TokenScanners.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        TokenScanners.GetEnumerator();
}
