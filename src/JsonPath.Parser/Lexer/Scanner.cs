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

        while (ctx.RemainingLength > 0)
        {
            var success = false;
            foreach (var scanner in subScanners)
            {
                if (scanner.TryScan(ref ctx, out Token token))
                {
                    success = true;
                    tokens.Add(token);
                    break;
                }
            }

            if (!success)
            {
                errors.Add(new JsonPathError(
                    DiagnosticPhase.Lexer,
                    "scanner",
                    "Couldn't recognize next token",
                    new SourceSpan(
                        ctx.Position,
                        ctx.RemainingLength),
                    allocator));
                break;
            }
        }

        return errors.IsEmpty;
    }
}