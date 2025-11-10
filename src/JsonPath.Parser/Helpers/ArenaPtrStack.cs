using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JsonPath.Parser.Helpers;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ArenaPtrStackHeader
{
    public int Count;
    public int Capacity;
    public void* Data; // points to a T* array
}

/// <summary>
/// A growable, non-allocating stack of pointers backed by an <see cref="ArenaAllocator"/>.
/// Safe to copy by value since metadata and buffer pointer are shared through the header.
/// </summary>
/// <typeparam name="T">Unmanaged element type (the stack stores pointers to this type).</typeparam>
public unsafe struct ArenaPtrStack<T>
    where T : unmanaged
{
    private readonly ArenaAllocator _arena;
    private ArenaPtrStackHeader* _header;

    public ArenaPtrStack(ArenaAllocator arena, int initialCapacity = 16)
    {
        if (initialCapacity <= 0)
        {
            initialCapacity = 1;
        }

        _arena = arena;

        _header = (ArenaPtrStackHeader*)arena.Alloc(
            (nuint)sizeof(ArenaPtrStackHeader),
            align: (nuint)IntPtr.Size);

        _header->Count = 0;
        _header->Capacity = initialCapacity;
        _header->Data = arena.Alloc(
            (nuint)initialCapacity * (nuint)sizeof(T*),
            align: (nuint)IntPtr.Size);
    }

    public bool IsEmpty => _header->Count == 0;
    public int Count => _header->Count;
    public int Capacity => _header->Capacity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T* value)
    {
        if (_header->Count >= _header->Capacity)
        {
            Grow();
        }

        var data = (T**)_header->Data;
        data[_header->Count++] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* Pop()
    {
        if (_header->Count == 0)
        {
            ThrowInvalidOperation("ArenaPtrStack underflow");
        }

        var data = (T**)_header->Data;
        return data[--_header->Count];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* Peek()
    {
        if (_header->Count == 0)
        {
            ThrowInvalidOperation("ArenaPtrStack empty");
        }

        var data = (T**)_header->Data;
        return data[_header->Count - 1];
    }

    public void Clear() => _header->Count = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow()
    {
        if (_header->Capacity > int.MaxValue / 2)
        {
            ThrowInvalidOperation("ArenaPtrStack capacity overflow");
        }

        var newCap = (nuint)_header->Capacity * 2;
        var newPtr = _arena.Alloc(newCap * (nuint)sizeof(T*), align: (nuint)IntPtr.Size);

        Unsafe.CopyBlockUnaligned(
            destination: newPtr,
            source: _header->Data,
            byteCount: (uint)(_header->Count * sizeof(T*)));

        _header->Data = newPtr;
        _header->Capacity = (int)newCap;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowInvalidOperation(string msg)
        => throw new InvalidOperationException(msg);
}
