using System.Runtime.InteropServices;

namespace JsonPath.Parser.Diagnostics;

[StructLayout(LayoutKind.Sequential)]
public readonly struct SourceSpan(int start, int length)
{
    public readonly int Start = start;
    public readonly int Length = length;

    public override string ToString() => $"[{Start}..{Start + Length})";
}