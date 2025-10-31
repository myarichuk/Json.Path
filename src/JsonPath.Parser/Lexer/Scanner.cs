using JsonPath.Parser.Allocators;
using JsonPath.Parser.Diagnostics;

namespace JsonPath.Parser.Lexer;

public readonly struct Scanner(
    ArenaAllocator allocator,
    SubScannerRepository subScanners)
{
    public bool TryScan(
        ReadOnlySpan<char> input,
        out ArenaList<Token> tokens,
        out ArenaList<JsonPathError> errors)
    {
        var ctx = new ScanContext(input);

        tokens = new ArenaList<Token>(allocator);
        errors = new ArenaList<JsonPathError>(allocator);

        // note: the lexer is WIP and not supposed to work yet
        throw new NotImplementedException();
    }
}