using JsonPath.Parser.Allocators;
using JsonPath.Parser.Diagnostics;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser.Lexer;

/// <summary>
/// Contract implemented by individual token scanners that try to consume input from a <see cref="ScanContext"/>.
/// </summary>
public interface ISubScanner
{
    /// <summary>
    /// Attempts to consume a token from the current position in the supplied context.
    /// </summary>
    /// <param name="ctx">The scanning context.</param>
    /// <param name="allocator">Allocator used for backing diagnostic collections.</param>
    /// <param name="errors">Collection populated when lexer diagnostics are emitted.</param>
    /// <param name="token">When successful, receives the token that was produced.</param>
    /// <returns><see langword="true"/> if a token was recognized; otherwise, <see langword="false"/>.</returns>
    bool TryScan(
        ref ScanContext ctx,
        ArenaAllocator allocator,
        ArenaList<JsonPathError> errors,
        out Token token);
}
