using System;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser;

/// <summary>
/// Writes metadata into existing <see cref="AstNode"/> instances and populates <see cref="AstDataTable"/>.
/// </summary>
public readonly unsafe ref struct AstWriteContext(ArenaAllocator allocator)
{
    /// <summary>
    /// Assigns structural node metadata for kinds that do not carry payloads.
    /// </summary>
    public void AssignNode(AstNode* node, AstKind kind)
    {
        node->Kind = kind;
        node->DataIndex = 0;
    }

    /// <summary>
    /// Assigns name-selector metadata using a managed string.
    /// </summary>
    public void AssignNameSelector(AstNode* node, ref AstDataTable dataTable, string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        AssignNameSelector(node, ref dataTable, ArenaString.Clone(name, allocator));
    }

    /// <summary>
    /// Assigns name-selector metadata using an arena-backed string.
    /// </summary>
    public void AssignNameSelector(AstNode* node, ref AstDataTable dataTable, in ArenaString name)
    {
        node->Kind = AstKind.NameSelector;
        node->DataIndex = dataTable.AddName(name);
    }

    /// <summary>
    /// Assigns literal metadata to a node.
    /// </summary>
    public void AssignLiteral(AstNode* node, ref AstDataTable dataTable, in LiteralData literal)
    {
        node->Kind = AstKind.LiteralValue;
        node->DataIndex = dataTable.AddLiteral(literal);
    }

    /// <summary>
    /// Assigns an index selector payload to a node.
    /// </summary>
    public void AssignIndexSelector(AstNode* node, ref AstDataTable dataTable, long index)
    {
        node->Kind = AstKind.IndexSelector;
        node->DataIndex = dataTable.AddIndex(index);
    }

    /// <summary>
    /// Assigns slice selector payload to a node.
    /// </summary>
    public void AssignSliceSelector(AstNode* node, ref AstDataTable dataTable, in SliceData slice)
    {
        node->Kind = AstKind.SliceSelector;
        node->DataIndex = dataTable.AddSlice(slice);
    }

    /// <summary>
    /// Assigns function metadata to a node.
    /// </summary>
    public void AssignFunctionCall(AstNode* node, ref AstDataTable dataTable, in FunctionCallData fn)
    {
        node->Kind = AstKind.FunctionCall;
        node->DataIndex = dataTable.AddFunction(fn);
    }
}
