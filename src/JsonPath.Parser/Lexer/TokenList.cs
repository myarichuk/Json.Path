using System.Runtime.CompilerServices;
using JsonPath.Parser.Allocators;

namespace JsonPath.Parser.Lexer;

public unsafe struct TokenList
{
    private Token* _base;

    private nuint _count = 0;
    private nuint _capacity;

    private readonly ArenaAllocator _arena;

    public TokenList(in ArenaAllocator arena, nuint initialCapacity = 128)
    {
        _base = (Token*)arena.Alloc(
            initialCapacity * (nuint)sizeof(Token),
            align: (nuint)Unsafe.SizeOf<Token>());
        _arena = arena;
        _capacity = initialCapacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in Token t)
    {
        if (_count >= _capacity)
        {
            Grow();
        }

        _base[_count++] = t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<Token> AsSpan() =>
        new(_base, (int)_count);

    private void Grow()
    {
        var newCap = _capacity * 2;
        var newPtr = (Token*)_arena.Alloc(newCap * (nuint)sizeof(Token), align: (nuint)Unsafe.SizeOf<Token>());

        Buffer.MemoryCopy(
            _base,
            newPtr,
            (long)newCap * sizeof(Token),
            (long)_count * sizeof(Token));
        _base = newPtr;
        _capacity = newCap;
    }
}