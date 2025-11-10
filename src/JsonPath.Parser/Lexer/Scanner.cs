using JsonPath.Parser.Allocators;
using JsonPath.Parser.Diagnostics;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser.Lexer;

/// <summary>
/// Provides the high level orchestration logic for lexing JsonPath expressions.
/// </summary>
public readonly ref struct Scanner
{
    private readonly ArenaAllocator _allocator;
    private readonly SubScannerRepository _subScanners;

    /// <summary>
    /// Provides the high level orchestration logic for lexing JsonPath expressions.
    /// </summary>
    /// <param name="allocator">Arena allocator used to back the token and error collections.</param>
    /// <param name="subScanners">Repository containing the concrete sub-scanners.</param>
    public Scanner(
        in ArenaAllocator allocator,
        SubScannerRepository subScanners)
    {
        _allocator = allocator;
        _subScanners = subScanners;
    }

    /// <summary>
    /// Attempts to tokenize the provided input.
    /// </summary>
    /// <param name="input">The source characters to scan.</param>
    /// <param name="tokens">The token list produced by the scan.</param>
    /// <param name="errors">Collection populated with lexer diagnostics, if any.</param>
    /// <returns><see langword="true"/> when the entire input is tokenized without errors; otherwise, <see langword="false"/>.</returns>
    public bool TryScan(
        ReadOnlySpan<char> input,
        ref AstDataTable dataTable,
        out ArenaList<Token> tokens,
        out ArenaList<JsonPathError> errors)
    {
        var ctx = new ScanContext(input);

        tokens = new ArenaList<Token>(_allocator);
        errors = new ArenaList<JsonPathError>(_allocator);

        while (ctx.RemainingLength > 0)
        {
            if (char.IsWhiteSpace(ctx.Current))
            {
                ctx.Consume();
                continue;
            }

            var success = false;
            foreach (var scanner in _subScanners)
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
                    _allocator));
                break;
            }
        }

        return errors.IsEmpty;
    }
}
