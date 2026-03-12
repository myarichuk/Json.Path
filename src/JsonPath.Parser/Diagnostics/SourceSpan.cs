using System.Runtime.InteropServices;

namespace JsonPath.Parser.Diagnostics;

/// <summary>
/// Represents a half-open span of characters within the source input.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct SourceSpan(int start, int length)
{
    /// <summary>
    /// Gets the zero-based starting offset of the span.
    /// </summary>
    public readonly int Start = start;

    /// <summary>
    /// Gets the length of the span.
    /// </summary>
    public readonly int Length = length;

    /// <summary>
    /// Returns a textual representation of the span using half-open interval notation.
    /// </summary>
    /// <returns>A string describing the span boundaries.</returns>
    public override string ToString() => $"[{Start}..{Start + Length})";
}