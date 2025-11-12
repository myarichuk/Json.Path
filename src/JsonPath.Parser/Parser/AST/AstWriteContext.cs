using System.Runtime.CompilerServices;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser;

/// <summary>
/// Writes metadata into existing <see cref="AstNode"/> instances and populates <see cref="AstDataTable"/>.
/// </summary>
public readonly unsafe ref struct AstWriteContext(ArenaAllocator allocator)
{
    /// <summary>
    /// Populates an already-allocated AST node with metadata from the provided payload.
    /// </summary>
    /// <typeparam name="T">Type of payload data.</typeparam>
    /// <param name="node">The node to populate. Must be allocated by the builder.</param>
    /// <param name="dataTable">The metadata table to write into.</param>
    /// <param name="kind">The AST kind.</param>
    /// <param name="data">The payload.</param>
    public void AssignNode<T>(AstNode* node, ref AstDataTable dataTable, AstKind kind, in T data)
    {
        node->Kind = kind;
        node->DataIndex = kind switch
        {
            AstKind.NameSelector => AddName(ref dataTable, data),
            AstKind.LiteralValue => AddLiteral(ref dataTable, data),
            AstKind.IndexSelector => AddIndex(ref dataTable, data),
            AstKind.SliceSelector => AddSlice(ref dataTable, data),
            AstKind.FunctionCall => AddFunction(ref dataTable, data),
            _ => 0u,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint AddName<T>(ref AstDataTable dataTable, in T data) => data switch
    {
        string s => dataTable.AddName(ArenaString.Clone(s, allocator)),
        ArenaString str => dataTable.AddName(str),
        _ => throw new ArgumentException(
            $"Invalid data type for NameSelector: {typeof(T)}"),
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint AddLiteral<T>(ref AstDataTable dataTable, in T data) =>
        data is LiteralData literal
            ? dataTable.AddLiteral(literal)
            : throw new ArgumentException(
                $"Invalid data type for LiteralValue: {typeof(T)}");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint AddIndex<T>(ref AstDataTable dataTable, in T data) =>
        data is long index
            ? dataTable.AddIndex(index)
            : throw new ArgumentException(
                $"Invalid data type for IndexSelector: {typeof(T)}");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint AddSlice<T>(ref AstDataTable dataTable, in T data) =>
        data is SliceData slice
            ? dataTable.AddSlice(slice)
            : throw new ArgumentException(
                $"Invalid data type for SliceSelector: {typeof(T)}");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint AddFunction<T>(ref AstDataTable dataTable, in T data) =>
        data is FunctionCallData fn
            ? dataTable.AddFunction(fn)
            : throw new ArgumentException(
                $"Invalid data type for FunctionCall: {typeof(T)}");
}
