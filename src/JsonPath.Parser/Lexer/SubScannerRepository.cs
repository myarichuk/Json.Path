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

    /// <summary>
    /// Returns an enumerator that iterates through the configured sub-scanners.
    /// </summary>
    /// <returns>An enumerator for the repository.</returns>
    public IEnumerator<ISubScanner> GetEnumerator() =>
        TokenScanners.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        TokenScanners.GetEnumerator();

    /// <summary>
    /// Gets the number of configured sub-scanners.
    /// </summary>
    public int Count => TokenScanners.Count;

    /// <summary>
    /// Gets the sub-scanner at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the scanner to retrieve.</param>
    /// <returns>The sub-scanner instance.</returns>
    public ISubScanner this[int index] => TokenScanners[index];
}

