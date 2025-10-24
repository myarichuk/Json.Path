using System.Collections;

namespace JsonPath.Parser.Lexer;

public class SubScannerRepository: IEnumerable<ISubScanner>
{
    private readonly List<ISubScanner> _tokenScanners = [];

    public SubScannerRepository()
    {
        foreach (var (literal, kind) in TokenKindExtensions.TokenLookup)
        {
            _tokenScanners.Add(new FixedStringScanner(literal, kind));
        }

        //TODO: don't forget to add other token scanners like one for identifiers
    }

    public IEnumerator<ISubScanner> GetEnumerator() =>
        _tokenScanners.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        _tokenScanners.GetEnumerator();
}