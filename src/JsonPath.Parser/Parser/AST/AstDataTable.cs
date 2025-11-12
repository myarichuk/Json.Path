using System;
using System.Runtime.CompilerServices;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser;

/// <summary>
/// Provides arena-backed storage for AST metadata referenced by <see cref="AstNode"/> instances.
/// </summary>
public struct AstDataTable(ArenaAllocator arena)
{
    private ArenaList<ArenaString> _names = new(arena);
    private ArenaList<LiteralData> _literals = new(arena);
    private ArenaList<long> _indices = new(arena);
    private ArenaList<SliceData> _slices = new(arena);
    private ArenaList<FunctionCallData> _functions = new(arena);
    private ArenaList<FunctionArgument> _functionArguments = new(arena);

    /// <summary>
    /// Adds a selector or identifier name to the backing store.
    /// </summary>
    /// <param name="name">The arena-backed string to record.</param>
    /// <returns>The index that can be used to retrieve the stored value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddName(in ArenaString name)
    {
        var idx = _names.Length;
        _names.Add(name);
        return (uint)idx;
    }

    /// <summary>
    /// Adds a literal value record to the backing store.
    /// </summary>
    /// <param name="literal">The literal metadata to add.</param>
    /// <returns>The index pointing to the persisted literal data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddLiteral(in LiteralData literal)
    {
        var idx = _literals.Length;
        _literals.Add(literal);
        return (uint)idx;
    }

    /// <summary>
    /// Adds an index selector value to the backing store.
    /// </summary>
    /// <param name="value">The numeric index value.</param>
    /// <returns>The index at which the selector was stored.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddIndex(long value)
    {
        var idx = _indices.Length;
        _indices.Add(value);
        return (uint)idx;
    }

    /// <summary>
    /// Adds a slice selector definition to the backing store.
    /// </summary>
    /// <param name="slice">The slice parameters to persist.</param>
    /// <returns>The index referencing the stored slice.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddSlice(in SliceData slice)
    {
        var idx = _slices.Length;
        _slices.Add(slice);
        return (uint)idx;
    }

    /// <summary>
    /// Adds a function call descriptor to the backing store.
    /// </summary>
    /// <param name="fn">The function call metadata to record.</param>
    /// <returns>The index referencing the stored function.</returns>
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

    /// <summary>
    /// Adds a function argument to the backing store and associates it with an existing function.
    /// </summary>
    /// <param name="functionIndex">Index of the function the argument belongs to.</param>
    /// <param name="argument">The argument metadata to append.</param>
    /// <exception cref="InvalidOperationException">Thrown when the argument limit is exceeded.</exception>
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

    /// <summary>
    /// Retrieves a view over the arguments for the specified function call.
    /// </summary>
    /// <param name="functionIndex">The index of the function whose arguments should be inspected.</param>
    /// <returns>A span that covers the stored function arguments, or an empty span when none exist.</returns>
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

    /// <summary>
    /// Retrieves a reference to the function argument at the specified index.
    /// </summary>
    /// <param name="functionIndex">The index of the function that owns the argument.</param>
    /// <param name="argumentIndex">The zero-based argument index.</param>
    /// <returns>A reference to the requested argument.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the argument index exceeds the stored count.</exception>
    public readonly ref FunctionArgument GetFunctionArgument(uint functionIndex, int argumentIndex)
    {
        ref var function = ref _functions[(int)functionIndex];
        if ((uint)argumentIndex >= function.ArgumentCount)
        {
            throw new ArgumentOutOfRangeException(nameof(argumentIndex));
        }

        return ref _functionArguments[(int)(function.ArgumentStartOffset + (uint)argumentIndex)];
    }

    /// <summary>
    /// Retrieves a reference to a stored name.
    /// </summary>
    /// <param name="index">Index of the name to access.</param>
    /// <returns>A reference to the stored <see cref="ArenaString"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref ArenaString GetName(uint index) => ref _names[(int)index];

    /// <summary>
    /// Retrieves a reference to a stored literal.
    /// </summary>
    /// <param name="index">Index of the literal to access.</param>
    /// <returns>A reference to the stored <see cref="LiteralData"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref LiteralData GetLiteral(uint index) => ref _literals[(int)index];

    /// <summary>
    /// Retrieves a reference to a stored index selector value.
    /// </summary>
    /// <param name="index">Index of the selector value to access.</param>
    /// <returns>A reference to the stored index value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref long GetIndex(uint index) => ref _indices[(int)index];

    /// <summary>
    /// Retrieves a reference to a stored slice descriptor.
    /// </summary>
    /// <param name="index">Index of the slice to access.</param>
    /// <returns>A reference to the stored <see cref="SliceData"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref SliceData GetSlice(uint index) => ref _slices[(int)index];

    /// <summary>
    /// Retrieves a reference to a stored function descriptor.
    /// </summary>
    /// <param name="index">Index of the function to access.</param>
    /// <returns>A reference to the stored <see cref="FunctionCallData"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref FunctionCallData GetFunction(uint index) => ref _functions[(int)index];

    /// <summary>
    /// Resets the table to an empty state while preserving allocated buffers.
    /// </summary>
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