using System.Runtime.CompilerServices;
using JsonPath.Parser.Helpers;

// ReSharper disable CheckNamespace
namespace JsonPath.Parser;

public readonly unsafe ref struct AstReadContext
{
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

    public T GetData<T>(ref AstDataTable data, AstNode* node)
    {
        if (node == null)
        {
            return default!;
        }

        return node->Kind switch
        {
            AstKind.NameSelector when typeof(T) == typeof(ArenaString) =>
                (T)(object)data.GetName(node->DataIndex),
            AstKind.LiteralValue when typeof(T) == typeof(LiteralData) =>
                (T)(object)data.GetLiteral(node->DataIndex),
            AstKind.IndexSelector when typeof(T) == typeof(long) =>
                (T)(object)data.GetIndex(node->DataIndex),
            AstKind.SliceSelector when typeof(T) == typeof(SliceData) =>
                (T)(object)data.GetSlice(node->DataIndex),
            AstKind.FunctionCall when typeof(T) == typeof(FunctionCallData) =>
                (T)(object)data.GetFunction(node->DataIndex),
            _ => throw new InvalidOperationException(
                $"Node kind {node->Kind} does not map to type {typeof(T).Name}"),
        };
    }
}