using System;
using System.Runtime.CompilerServices;

namespace JsonPath.Parser.Allocators;

/// <summary>
/// A growable bump allocator that allocates memory from chained unmanaged segments.
/// </summary>
public unsafe class ArenaAllocator : IDisposable
{
    private const nuint DefaultSegmentSize = 10 * 1024 * 1024; // 10 MB
    private const nuint MaxSegmentSize = 256 * 1024 * 1024;    // 256 MB cap
    private static readonly nuint DefaultAlignment = (nuint)IntPtr.Size;

    private readonly ArenaSegment* _first;
    private ArenaSegment* _current;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArenaAllocator"/> class.
    /// </summary>
    /// <param name="initialSize">Size of the first segment to be allocated</param>
    /// <remarks>Each new segment would have it's size doubled (up to a cap)</remarks>
    public ArenaAllocator(nuint initialSize = DefaultSegmentSize)
    {
        _first = AllocateNew(initialSize);
        _current = _first;
    }

    /// <summary>
    /// Allocates a block of unmanaged memory from the arena.
    /// Grows automatically when current segment runs out of space.
    /// </summary>
    /// <returns>Pointer to a new segment</returns>
    public void* Alloc(nuint size, nuint align = 0)
    {
        while (true)
        {
            if (_current->TryAlloc(size, align == 0 ? DefaultAlignment : align, out var ptr))
            {
                return ptr;
            }

            var newSeg = AllocateNew(size);
            _current->Next = newSeg;
            _current = newSeg;
        }
    }

    private ArenaSegment* AllocateNew(nuint requestSize)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ArenaAllocator));
        }

        var prevSize = _current is null ? 0 : _current->Size;
        var newSize = NextSegmentSize(prevSize, requestSize);

        var seg = (ArenaSegment*)NativeAllocator.Alloc((nuint)sizeof(ArenaSegment));
        seg->Base = (byte*)NativeAllocator.Alloc(newSize);
        seg->Size = newSize;
        seg->Next = null;

        return seg;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nuint NextSegmentSize(nuint prevSize, nuint request)
    {
        var baseSize = prevSize == 0 ?
            DefaultSegmentSize : Math.Min(prevSize * 2, MaxSegmentSize);
        return request > baseSize ? AlignUp(request, 4096) : baseSize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nuint AlignUp(nuint value, nuint align) =>
        (value + (align - 1)) & ~(align - 1);

    private void Free(ArenaSegment* seg)
    {
        NativeAllocator.Free(seg->Base);
        NativeAllocator.Free(seg);
    }

    private void ReleaseUnmanagedResources()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        var seg = _first;
        while (seg != null)
        {
            var next = seg->Next;
            Free(seg);
            seg = next;
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~ArenaAllocator() => ReleaseUnmanagedResources();
}
