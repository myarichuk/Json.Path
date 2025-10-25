using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace JsonPath.Parser.Allocators;

/// <summary>
/// A growable bump allocator that allocates memory from chained unmanaged segments.
/// </summary>
public unsafe class ArenaAllocator : IDisposable
{
    private const nuint DefaultSegmentSize = 10 * 1024 * 1024; // 10 MB
    private const nuint MaxSegmentSize = 256 * 1024 * 1024;    // 256 MB cap
    private static readonly nuint DefaultAlignment = (nuint)IntPtr.Size;

    private readonly object _globalLock = new object();
    private ArenaSegment* _first;
    private ArenaSegment* _current;
    private bool _disposed;
    private int _disposeState; // 0=alive, 1=disposing, 2=disposed

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
        align = align == 0 ? DefaultAlignment : align;

        lock (_globalLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_current is null)
            {
                var initial = AllocateNew(size);

                if (_first is null)
                {
                    _first = initial;
                }

                _current = initial;
            }

            while (true)
            {
                var current = _current;

                if (current is not null && current->TryAlloc(size, align, out var ptr))
                {
                    return ptr;
                }

                var newSeg = AllocateNew(size);

                if (_first is null)
                {
                    _first = newSeg;
                }
                else if (current is not null)
                {
                    current->Next = newSeg;
                }

                _current = newSeg;
            }
        }
    }

    private ArenaSegment* AllocateNew(nuint requestSize)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var prevSize = _current is null ? 0 : _current->Size;
        var newSize = NextSegmentSize(prevSize, requestSize);

        var seg = (ArenaSegment*)NativeAllocator.Alloc((nuint)sizeof(ArenaSegment));
        *seg = default; // ensure _offset = 0
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


    private void ReleaseUnmanagedResources()
    {
        lock (_globalLock)
        {
            var cur = _first;
            _first = null;
            _current = null;

            while (cur != null)
            {
                var next = cur->Next;

                if (cur->Base != null)
                {
                    NativeAllocator.Free(cur->Base);
                    cur->Base = null;
                }

                NativeAllocator.Free(cur);
                cur = next;
            }
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposeState, 1) != 0)
        {
            return;
        }

        _disposed = true;
        lock (_globalLock)
        {
            ReleaseUnmanagedResources();
        }

        Volatile.Write(ref _disposeState, 2);
        GC.SuppressFinalize(this);
    }

    ~ArenaAllocator()
    {
        if (Interlocked.Exchange(ref _disposeState, 1) != 0)
        {
            return;
        }

        try
        {
            ReleaseUnmanagedResources();
        }
        finally
        {
            Volatile.Write(ref _disposeState, 2);
        }
    }
}