using System.Collections;

namespace JsonPath.Parser.Lexer;

/// <summary>
/// Provides the ordered set of sub-scanners that the <see cref="Scanner"/> consults.
/// </summary>
public class SubScannerRepository : IReadOnlyList<ISubScanner>
{
    private static readonly IReadOnlyList<ISubScanner> TokenScanners;

    static SubScannerRepository()
    {
        var scanners = new List<ISubScanner>(TokenKindExtensions.TokenLookup.Count);

        foreach (var (literal, kind) in TokenKindExtensions.TokenLookup
                     .OrderByDescending(x => x.Key.Length))
        {
            scanners.Add(new TokenScanner(literal, kind));
        }

        scanners.Add(new StringScanner());
        scanners.Add(new IdentifierScanner());
        scanners.Add(new NumberScanner());

        TokenScanners = scanners;
    }

    public IEnumerator<ISubScanner> GetEnumerator() =>
        TokenScanners.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        TokenScanners.GetEnumerator();

    public int Count => TokenScanners.Count;

    public ISubScanner this[int index] => TokenScanners[index];
}

