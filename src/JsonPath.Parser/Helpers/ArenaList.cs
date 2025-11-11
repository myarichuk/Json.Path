using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JsonPath.Parser.Helpers;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ArenaListHeader
{
    public int Count;
    public int Capacity;
    public void* Data;
}


/// <summary>
/// A simple, arena-backed continuous list for unmanaged structs.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ArenaList<T>
    where T : unmanaged
{
    private readonly ArenaAllocator _arena; // class reference – fine
    private ArenaListHeader* _header;

    public ArenaList(ArenaAllocator arena, int initialCapacity = 16)
    {
        _arena = arena;
        _header = (ArenaListHeader*)arena.Alloc((nuint)sizeof(ArenaListHeader), align: (nuint)IntPtr.Size);
        _header->Count = 0;
        _header->Capacity = initialCapacity;
        _header->Data = (T*)arena.Alloc((nuint)initialCapacity * (nuint)sizeof(T));
    }

    public ref T this[int index]
    {
        get
        {
            Debug.Assert(index >= 0 && (uint)index < (uint)_header->Count, "out of bounds for ArenaList indexer");
            return ref ((T*)_header->Data)[index];
        }
    }

    public int Length => _header != null ? _header->Count : 0;

    public void Add(in T value)
    {
        if (_header->Count >= _header->Capacity)
        {
            Grow();
        }

        ((T*)_header->Data)[_header->Count++] = value;
    }

    private void Grow()
    {
        var newCap = (nuint)_header->Capacity * 2;
        var newPtr = _arena.Alloc(newCap * (nuint)sizeof(T));
        Unsafe.CopyBlockUnaligned(newPtr, _header->Data, (uint)(_header->Count * sizeof(T)));
        _header->Data = newPtr;
        _header->Capacity = (int)newCap;
    }

    public bool IsEmpty => _header == null || _header->Count == 0;
    
    public void Reset()
    {
        // just in case :)
        if (_header == null)
        {
            return;
        }

        _header->Count = 0;
    }

    public ReadOnlySpan<T> AsSpan() => new((T*)_header->Data, _header->Count);
}
