using System.Runtime.InteropServices;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser.Diagnostics;

/// <summary>
/// Represents a parser or lexer diagnostic emitted during JsonPath processing.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct JsonPathError(
    DiagnosticPhase phase,
    ReadOnlySpan<char> code,
    ReadOnlySpan<char> message,
    SourceSpan span,
    ArenaAllocator arena)
{
    /// <summary>
    /// Gets the phase of the pipeline that produced the error.
    /// </summary>
    public readonly DiagnosticPhase Phase = phase;

    /// <summary>
    /// Gets the symbolic error code.
    /// </summary>
    public readonly ArenaString Code = ArenaString.Clone(code, arena);

    /// <summary>
    /// Gets the human readable diagnostic message.
    /// </summary>
    public readonly ArenaString Message = ArenaString.Clone(message, arena);

    /// <summary>
    /// Gets the source span associated with the error.
    /// </summary>
    public readonly SourceSpan Span = span;

    /// <summary>
    /// Returns a textual representation of the diagnostic.
    /// </summary>
    /// <returns>A formatted string containing phase, code, message, and span.</returns>
    public override string ToString() =>
        $"{Phase}: {Code} - {Message} at {Span}";
}