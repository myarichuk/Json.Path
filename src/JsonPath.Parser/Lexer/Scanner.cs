using JsonPath.Parser.Allocators;
using JsonPath.Parser.Diagnostics;

namespace JsonPath.Parser.Lexer;

public ref struct Scanner
{
    private readonly ReadOnlySpan<char> _input;
    private readonly ArenaAllocator _allocator;
    private readonly SubScannerRepository _subScanners;

    public Scanner(
        ReadOnlySpan<char> input,
        ArenaAllocator allocator,
        SubScannerRepository subScanners)
    {
        _input = input;
        _allocator = allocator;
        _subScanners = subScanners;
    }

    public bool TryScan(out ArenaList<Token> tokens, out ArenaList<JsonPathError> errors)
    {
        tokens = new ArenaList<Token>(_allocator);
        errors = new ArenaList<JsonPathError>(_allocator);
        throw new NotImplementedException();
    }
}