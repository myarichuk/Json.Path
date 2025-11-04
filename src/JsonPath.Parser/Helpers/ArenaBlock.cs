using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JsonPath.Parser.Helpers;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ArenaBlock<T>
    where T : unmanaged
{
    public T* Data;
    public nuint Count;
    public nuint Capacity;
    public ArenaBlock<T>* Next;
}

public unsafe struct ArenaBlockList<T> : IEnumerable<T>
    where T : unmanaged
{
    private readonly ArenaAllocator _arena;
    private readonly ArenaBlock<T>* _head;
    private ArenaBlock<T>* _current;

    private const nuint DefaultBlockSize = 128;

    public ArenaBlockList(ArenaAllocator arena, nuint blockSize = DefaultBlockSize)
    {
        _arena = arena;
        _head = _current = CreateBlock(arena, blockSize);
    }

    public void Add(in T value)
    {
        if (_current->Count >= _current->Capacity)
        {
            var newBlock = CreateBlock(_arena, _current->Capacity * 2);
            _current->Next = newBlock;
            _current = newBlock;
        }

        _current->Data[_current->Count++] = value;
    }

    private static ArenaBlock<T>* CreateBlock(ArenaAllocator arena, nuint capacity)
    {
        nuint headerSize = (nuint)sizeof(ArenaBlock<T>);
        nuint dataSize = capacity * (nuint)sizeof(T);
        byte* mem = (byte*)arena.Alloc(headerSize + dataSize);
        var block = (ArenaBlock<T>*)mem;
        block->Data = (T*)(mem + headerSize);
        block->Count = 0;
        block->Capacity = capacity;
        block->Next = null;
        return block;
    }

    // TODO: cache in memory - we update it any way when adding
    public nuint Count
    {
        get
        {
            nuint total = 0;
            for (var b = _head; b != null; b = b->Next)
            {
                total += b->Count;
            }

            return total;
        }
    }

    public nuint Capacity
    {
        get
        {
            nuint total = 0;
            for (var b = _head; b != null; b = b->Next)
            {
                total += b->Capacity;
            }

            return total;
        }
    }

    public void Reset()
    {
        for (var b = _head; b != null; b = b->Next)
        {
            b->Count = 0;
        }

        _current = _head;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(_head);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(_head);

    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(_head);

    public struct Enumerator(ArenaBlock<T>* head) : IEnumerator<T>
    {
        private readonly ArenaBlock<T>* _head = head;
        private ArenaBlock<T>* _block = head;
        private nuint _index = 0;
        private T _current = default;

        /// <inheritdoc />
        public T Current => _current;

        /// <inheritdoc/>
        object IEnumerator.Current => _current!;

        /// <inheritdoc />
        public bool MoveNext()
        {
            if (_block == null)
            {
                return false;
            }

            while (_block != null)
            {
                if (_index < _block->Count)
                {
                    _current = _block->Data[_index++];
                    return true;
                }

                _block = _block->Next;
                _index = 0;
            }

            return false;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _block = _head;
            _index = 0;
            _current = default;
        }

        /// <inheritdoc />
        public void Dispose() { }
    }

    public ReadOnlySpan<T> GetSpan()
    {
        var total = Count;
        if (total == 0)
        {
            return ReadOnlySpan<T>.Empty;
        }

        var buffer = (T*)_arena.Alloc(total * (nuint)sizeof(T));
        var dst = buffer;
        for (var block = _head; block != null; block = block->Next)
        {
            for (nuint i = 0; i < block->Count; i++)
            {
                *dst++ = block->Data[i];
            }
        }

        return new ReadOnlySpan<T>(buffer, (int)total);
    }
}