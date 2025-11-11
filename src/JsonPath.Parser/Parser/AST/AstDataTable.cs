using System;
using System.Runtime.CompilerServices;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser;

public struct AstDataTable(ArenaAllocator arena)
{
    private ArenaList<ArenaString> _names = new(arena);
    private ArenaList<LiteralData> _literals = new(arena);
    private ArenaList<long> _indices = new(arena);
    private ArenaList<SliceData> _slices = new(arena);
    private ArenaList<FunctionCallData> _functions = new(arena);
    private ArenaList<FunctionArgument> _functionArguments = new(arena);
    private readonly ArenaAllocator _arena = arena;

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
        var storedFn = fn;
        storedFn.ArgumentStartOffset = uint.MaxValue;
        storedFn.ArgumentCount = 0;
        _functions.Add(storedFn);
        return (uint)idx;
    }

    public void AddFunctionArgument(uint functionIndex, in FunctionArgument argument)
    {
        ref var function = ref _functions[(int)functionIndex];

        if (function.ArgumentCount == ushort.MaxValue)
        {
            throw new InvalidOperationException("Function argument count exceeded supported limit.");
        }

        var expectedIndex = (int)_functionArguments.Length;
        if (function.ArgumentCount == 0)
        {
            function.ArgumentStartOffset = (uint)expectedIndex;
        }
        else if (function.ArgumentStartOffset + function.ArgumentCount != (uint)expectedIndex)
        {
            var oldStart = function.ArgumentStartOffset;
            var oldCount = function.ArgumentCount;
            var newStart = (uint)_functionArguments.Length;

            for (var i = 0; i < oldCount; i++)
            {
                var existingArgument = _functionArguments[(int)(oldStart + (uint)i)];
                _functionArguments.Add(existingArgument);
            }

            function.ArgumentStartOffset = newStart;
        }

        _functionArguments.Add(argument);
        function.ArgumentCount++;
    }

    public readonly ReadOnlySpan<FunctionArgument> GetFunctionArguments(uint functionIndex)
    {
        ref readonly var function = ref _functions[(int)functionIndex];
        if (function.ArgumentCount == 0)
        {
            return ReadOnlySpan<FunctionArgument>.Empty;
        }

        var start = (int)function.ArgumentStartOffset;
        return _functionArguments.AsSpan().Slice(start, function.ArgumentCount);
    }

    public readonly ref FunctionArgument GetFunctionArgument(uint functionIndex, int argumentIndex)
    {
        ref var function = ref _functions[(int)functionIndex];
        if ((uint)argumentIndex >= function.ArgumentCount)
        {
            throw new ArgumentOutOfRangeException(nameof(argumentIndex));
        }

        return ref _functionArguments[(int)(function.ArgumentStartOffset + (uint)argumentIndex)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref ArenaString GetName(uint index) => ref _names[(int)index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref LiteralData GetLiteral(uint index) => ref _literals[(int)index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref long GetIndex(uint index) => ref _indices[(int)index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref SliceData GetSlice(uint index) => ref _slices[(int)index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref FunctionCallData GetFunction(uint index) => ref _functions[(int)index];

    public void Reset()
    {
        _names.Reset();
        _literals.Reset();
        _indices.Reset();
        _slices.Reset();
        _functions.Reset();
        _functionArguments.Reset();
    }
}