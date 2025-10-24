using System.Runtime.InteropServices;
using JsonPath.Parser.Allocators;

namespace JsonPath.Parser.Diagnostics;

[StructLayout(LayoutKind.Sequential)]
public readonly struct JsonPathError(
    DiagnosticPhase phase,
    ReadOnlySpan<char> code,
    ReadOnlySpan<char> message,
    SourceSpan span,
    ArenaAllocator arena)
{
    public readonly DiagnosticPhase Phase = phase;
    public readonly ArenaString Code = ArenaString.Clone(code, arena);
    public readonly ArenaString Message = ArenaString.Clone(message, arena);
    public readonly SourceSpan Span = span;

    public override string ToString() =>
        $"{Phase}: {Code} - {Message} at {Span}";
}