using System.Runtime.CompilerServices;

namespace JsonPath.Parser.Helpers;

/// <summary>
/// A simple, arena-backed continuous list for unmanaged structs.
/// </summary>
public unsafe struct ArenaList<T>(in ArenaAllocator arena, int initialCapacity = 16)
    where T : unmanaged
{
    private T* _base =
        (T*)arena.Alloc(
            (nuint)initialCapacity * (nuint)sizeof(T),
            align: (nuint)IntPtr.Size);

    private int _count = 0;
    private int _capacity = initialCapacity;
    private readonly ArenaAllocator _arena = arena;

    public bool IsEmpty => _count == 0;

    public int Length
    {
        get => _count;
        set => _count = value;
    }

    public ref T this[int index] => ref _base[index];

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

    public void Reset() =>
        _count = 0; // no need to delete stuff, just assume we are empty!

    private void Grow()
    {
        var newCap = (nuint)_capacity * 2;
        var newPtr = (T*)_arena.Alloc(newCap * (nuint)sizeof(T));
        Buffer.MemoryCopy(
            _base,
            newPtr,
            (long)newCap * sizeof(T),
            (long)_count * sizeof(T));

        _base = newPtr;
        _capacity = (int)newCap;
    }
}
