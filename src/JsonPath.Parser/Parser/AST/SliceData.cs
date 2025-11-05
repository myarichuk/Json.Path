using System.Runtime.InteropServices;

namespace JsonPath.Parser;

[Flags]
public enum SliceFlags : byte
{
    None = 0,
    HasStart = 1,
    HasEnd = 2,
    HasStep = 4
}

[StructLayout(LayoutKind.Sequential)]
public struct SliceData
{
    public int Start;    // inclusive
    public int End;      // exclusive
    public int Step;     // step
    public SliceFlags Flags; // which ones were present

    public readonly bool HasStart => (Flags & SliceFlags.HasStart) != 0;
    public readonly bool HasEnd => (Flags & SliceFlags.HasEnd) != 0;
    public readonly bool HasStep => (Flags & SliceFlags.HasStep) != 0;

    public readonly override string ToString()
    {
        var s = HasStart ? Start.ToString() : string.Empty;
        var e = HasEnd ? End.ToString() : string.Empty;
        var st = HasStep ? Step.ToString() : string.Empty;
        return $"[{s}:{e}:{st}]";
    }
}