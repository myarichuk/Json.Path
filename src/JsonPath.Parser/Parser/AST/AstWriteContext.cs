using System.Runtime.CompilerServices;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser;

/// <summary>
/// Facilitates writing <see cref="AstNode"/> instances and associated metadata into an <see cref="AstDataTable"/>.
/// </summary>
public readonly unsafe ref struct AstWriteContext(in ArenaAllocator allocator)
{
    private readonly ArenaAllocator _allocator = allocator;

    /// <summary>
    /// Creates a node populated with metadata from the provided data payload.
    /// </summary>
    /// <typeparam name="T">The type of data being associated with the node.</typeparam>
    /// <param name="dataTable">Destination table for metadata storage.</param>
    /// <param name="kind">The AST kind represented by the node.</param>
    /// <param name="data">The payload backing the node.</param>
    /// <returns>A pointer to the allocated node.</returns>
    public AstNode* CreateNode<T>(ref AstDataTable dataTable, AstKind kind, in T data)
    {
        var node = AllocNode(kind);
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
        return node;
    }

    private AstNode* AllocNode(AstKind kind)
    {
        var node = (AstNode*)_allocator.Alloc((nuint)sizeof(AstNode));
        node->Kind = kind;
        node->NextChild = null;
        node->NextSibling = null;
        node->DataIndex = 0;
        return node;
    }

    // --- inline data writers ---
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint AddName<T>(ref AstDataTable dataTable, in T data) => data switch
    {
        string s => dataTable.AddName(ArenaString.Clone(s, _allocator)),
        ArenaString str => dataTable.AddName(str),
        _ => throw new ArgumentException($"Invalid data type for {nameof(AstKind.NameSelector)}: {typeof(T)}"),
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint AddLiteral<T>(ref AstDataTable dataTable, in T data) =>
        data is LiteralData literal ? dataTable.AddLiteral(literal)
        : throw new ArgumentException($"Invalid data type for {nameof(AstKind.LiteralValue)}: {typeof(T)}");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint AddIndex<T>(ref AstDataTable dataTable, in T data) =>
        data is long index ? dataTable.AddIndex(index)
        : throw new ArgumentException($"Invalid data type for {nameof(AstKind.IndexSelector)}: {typeof(T)}");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint AddSlice<T>(ref AstDataTable dataTable, in T data) =>
        data is SliceData slice ? dataTable.AddSlice(slice)
        : throw new ArgumentException($"Invalid data type for {nameof(AstKind.SliceSelector)}: {typeof(T)}");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint AddFunction<T>(ref AstDataTable dataTable, in T data) =>
        data is FunctionCallData fn ? dataTable.AddFunction(fn)
        : throw new ArgumentException($"Invalid data type for {nameof(AstKind.FunctionCall)}: {typeof(T)}");
}