using System;
using System.Runtime.CompilerServices;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser;

public unsafe struct AstDataTable
{
    private ArenaList<ArenaString> _names;
    private ArenaList<LiteralData> _literals;
    private ArenaList<long> _indices;
    private ArenaList<SliceData> _slices;
    private ArenaList<FunctionCallData> _functions;
    private readonly ArenaAllocator _arena;

    public AstDataTable(ArenaAllocator arena)
    {
        _arena = arena;
        _names = new ArenaList<ArenaString>(arena);
        _literals = new ArenaList<LiteralData>(arena);
        _indices = new ArenaList<long>(arena);
        _slices = new ArenaList<SliceData>(arena);
        _functions = new ArenaList<FunctionCallData>(arena);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddName(in ArenaString name)
    {
        var idx = _names.Length;
        _names.Add(name);
        return (uint)idx;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddLiteral(in LiteralData literal)
    {
        var idx = _literals.Length;
        _literals.Add(literal);
        return (uint)idx;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddIndex(long value)
    {
        var idx = _indices.Length;
        _indices.Add(value);
        return (uint)idx;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddSlice(in SliceData slice)
    {
        var idx = _slices.Length;
        _slices.Add(slice);
        return (uint)idx;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddFunction(in FunctionCallData fn)
    {
        var idx = _functions.Length;
        _functions.Add(fn);
        return (uint)idx;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ArenaString GetName(uint index) => _names[(int)index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LiteralData GetLiteral(uint index) => _literals[(int)index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long GetIndex(uint index) => _indices[(int)index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SliceData GetSlice(uint index) => _slices[(int)index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FunctionCallData GetFunction(uint index) => _functions[(int)index];

    public void Reset()
    {
        _names.Reset();
        _literals.Reset();
        _indices.Reset();
        _slices.Reset();
        _functions.Reset();
    }
}