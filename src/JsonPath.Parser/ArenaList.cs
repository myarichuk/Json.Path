using System.Runtime.CompilerServices;
using JsonPath.Parser.Allocators;

namespace JsonPath.Parser;

/// <summary>
/// A simple, arena-backed continuous list for unmanaged structs.
/// </summary>
public unsafe struct ArenaList<T>(in ArenaAllocator arena, nuint initialCapacity = 16)
    where T : unmanaged
{
    private T* _base =
        (T*)arena.Alloc(
            initialCapacity * (nuint)sizeof(T),
            align: (nuint)IntPtr.Size);

    private nuint _count = 0;
    private nuint _capacity = initialCapacity;
    private readonly ArenaAllocator _arena = arena;

    public nuint Count => _count;

    public bool IsEmpty => _count == 0;

    public ref T this[nuint index] => ref _base[index];

    public ReadOnlySpan<T> AsSpan() => new(_base, (int)_count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in T value)
    {
        if (_count >= _capacity)
        {
            Grow();
        }

        _base[_count++] = value;
    }

    private void Grow()
    {
        var newCap = _capacity * 2;
        var newPtr = (T*)_arena.Alloc(newCap * (nuint)sizeof(T));
        Buffer.MemoryCopy(
            _base,
            newPtr,
            (long)newCap * sizeof(T),
            (long)_count * sizeof(T));

        _base = newPtr;
        _capacity = newCap;
    }
}
