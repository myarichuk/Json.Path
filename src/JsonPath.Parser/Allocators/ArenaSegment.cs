using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JsonPath.Parser.Allocators;

/// <summary>
/// Represents a single contiguous segment of memory in an arena.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ArenaSegment
{
    private nuint _offset;

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

        var aligned = (_offset + (align - 1)) & ~(align - 1);
        if (aligned + size > Size)
        {
            ptr = null;
            return false;
        }

        ptr = Base + aligned;
        _offset = aligned + size;
        return true;
    }
}