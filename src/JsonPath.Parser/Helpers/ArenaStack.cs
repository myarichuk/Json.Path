using System.Runtime.CompilerServices;

namespace JsonPath.Parser.Helpers;

public unsafe struct ArenaStack<T>(in ArenaAllocator arena, int initialCapacity = 16)
    where T : unmanaged
{
    private T** _base =
        (T**)arena.Alloc(
            (nuint)initialCapacity * (nuint)sizeof(T*),
            align: (nuint)IntPtr.Size);

    private int _count = 0;
    private int _capacity = initialCapacity;
    private readonly ArenaAllocator _arena = arena;

    public bool IsEmpty => _count == 0;
    public int Count => _count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T* value)
    {
        if (_count >= _capacity)
        {
            Grow();
        }

        _base[_count++] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* Pop()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("ArenaStack underflow");
        }

        return _base[--_count];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* Peek()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("ArenaStack empty");
        }

        return _base[_count - 1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => _count = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow()
    {
        var newCap = (nuint)_capacity * 2;
        var newPtr = (T**) _arena.Alloc(newCap * (nuint)sizeof(T*));

        Buffer.MemoryCopy(
            _base,
            newPtr,
            (long)newCap * sizeof(T*),
            (long)_count * sizeof(T*));

        _base = newPtr;
        _capacity = (int)newCap;
    }
}
