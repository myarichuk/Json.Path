using System.Runtime.CompilerServices;
using JsonPath.Parser.Helpers;

// ReSharper disable CheckNamespace
namespace JsonPath.Parser;

/// <summary>
/// Provides helper methods for reading strongly typed data from an <see cref="AstDataTable"/>.
/// </summary>
public readonly unsafe ref struct AstReadContext
{
    /// <summary>
    /// Retrieves the object representation associated with a node, or <see langword="null"/> when the node is absent.
    /// </summary>
    /// <param name="data">The table containing AST metadata.</param>
    /// <param name="node">The node describing the desired data.</param>
    /// <returns>The boxed data represented by <paramref name="node"/>, or <see langword="null"/>.</returns>
    public object? GetData(ref AstDataTable data, AstNode* node)
    {
        if (node == null)
        {
            return null;
        }

        return node->Kind switch
        {
            AstKind.NameSelector => data.GetName(node->DataIndex),
            AstKind.LiteralValue => data.GetLiteral(node->DataIndex),
            AstKind.IndexSelector => data.GetIndex(node->DataIndex),
            AstKind.SliceSelector => data.GetSlice(node->DataIndex),
            AstKind.FunctionCall => data.GetFunction(node->DataIndex),
            _ => null,
        };
    }

    /// <summary>
    /// Retrieves the typed representation associated with a node.
    /// </summary>
    /// <typeparam name="T">The expected data type.</typeparam>
    /// <param name="data">The table containing AST metadata.</param>
    /// <param name="node">The node describing the desired data.</param>
    /// <returns>The requested data typed as <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the node kind does not match <typeparamref name="T"/>.</exception>
    public T GetData<T>(ref AstDataTable data, AstNode* node)
    {
        if (node == null)
        {
            return default!;
        }

        return node->Kind switch
        {
            AstKind.NameSelector when typeof(T) == typeof(ArenaString) =>
                Unsafe.As<ArenaString, T>(ref data.GetName(node->DataIndex)),
            AstKind.LiteralValue when typeof(T) == typeof(LiteralData) =>
                Unsafe.As<LiteralData, T>(ref data.GetLiteral(node->DataIndex)),
            AstKind.IndexSelector when typeof(T) == typeof(long) =>
                Unsafe.As<long, T>(ref data.GetIndex(node->DataIndex)),
            AstKind.SliceSelector when typeof(T) == typeof(SliceData) =>
                Unsafe.As<SliceData, T>(ref data.GetSlice(node->DataIndex)),
            AstKind.FunctionCall when typeof(T) == typeof(FunctionCallData) =>
                Unsafe.As<FunctionCallData, T>(ref data.GetFunction(node->DataIndex)),
            _ => throw new InvalidOperationException(
                $"Node kind {node->Kind} does not map to type {typeof(T).Name}"),
        };
    }
}