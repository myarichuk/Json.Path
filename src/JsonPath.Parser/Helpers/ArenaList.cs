using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JsonPath.Parser.Helpers;

[StructLayout(LayoutKind.Sequential)]
public struct ArenaListHeader
{
    public int Count;
    public int Capacity;
}

/// <summary>
/// A simple, arena-backed continuous list for unmanaged structs.
/// </summary>
public unsafe struct ArenaList<T>
    where T : unmanaged
{
    private readonly ArenaAllocator _arena; // class reference – fine
    private ArenaListHeader* _header;
    private T* _data;

    public ArenaList(ArenaAllocator arena, int initialCapacity = 16)
    {
        _arena = arena;
        _header = (ArenaListHeader*)arena.Alloc((nuint)sizeof(ArenaListHeader), align: (nuint)IntPtr.Size);
        _header->Count = 0;
        _header->Capacity = initialCapacity;
        _data = (T*)arena.Alloc((nuint)initialCapacity * (nuint)sizeof(T));
    }

    public ref T this[int index] => ref _data[index];

    public int Length => _header != null ? _header->Count : 0;

    public void Add(in T value)
    {
        if (_header->Count >= _header->Capacity)
            Grow();

        _data[_header->Count++] = value;
    }

    private void Grow()
    {
        var newCap = (nuint)_header->Capacity * 2;
        var newPtr = (T*)_arena.Alloc(newCap * (nuint)sizeof(T));
        Buffer.MemoryCopy(_data, newPtr, (long)newCap * sizeof(T), (long)_header->Count * sizeof(T));
        _data = newPtr;
        _header->Capacity = (int)newCap;
    }

    public bool IsEmpty => _header == null || _header->Count == 0;
    
    public void Reset() => _header->Count = 0;

    public ReadOnlySpan<T> AsSpan() => new(_data, _header->Count);
}
