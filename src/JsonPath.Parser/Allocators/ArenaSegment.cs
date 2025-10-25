using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JsonPath.Parser.Allocators;

/// <summary>
/// Represents a single contiguous segment of memory in an arena.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ArenaSegment
{
#if DEBUG
    public ulong HeadCanary;
    public ulong TailCanary;

    internal const ulong Canary = 0xDEADBEEFCAFEBABEul;
#endif

    public nuint Offset;
    public byte* Base;
    public nuint Size;
    public ArenaSegment* Next;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAlloc(nuint size, nuint align, out void* ptr)
    {
        if (align == 0)
        {
            align = (nuint)IntPtr.Size;
        }

        Debug.Assert((align & (align - 1)) == 0, "align must be a power of two");

        // round up
        var aligned = (Offset + (align - 1)) & ~(align - 1);

        // overflow-safe bound check: aligned <= Size - size
        if (size > Size || aligned > Size - size)
        {
            ptr = null;
            return false;
        }

        ptr = Base + aligned;
        Offset = aligned + size;
        return true;
    }
}