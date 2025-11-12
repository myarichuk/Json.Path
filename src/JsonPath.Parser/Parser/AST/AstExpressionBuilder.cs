using System;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser;

public unsafe ref struct AstExpressionBuilder(
    ArenaAllocator allocator,
    AstBuilder builder,
    AstDataTable data,
    AstHandle current)
{
    private readonly AstWriteContext _write = new(allocator);
    private AstBuilder _builder = builder;
    private AstDataTable _data = data;
    private AstHandle _current = current;

    /// <summary>
    /// Begins a nested expression node: allocates it, attaches metadata, and descends.
    /// </summary>
    public AstExpressionBuilder BeginExpression<TData>(AstKind kind, in TData data)
    {
        // allocate + push into the AST
        var handle = _builder.Begin(kind);

        // attach data to that node
        _write.AssignNode(handle.Ptr, ref _data, kind, data);

        _current = handle;
        return this;
    }

    /// <summary>
    /// Ends the current nested expression scope.
    /// </summary>
    public AstExpressionBuilder EndExpression()
    {
        _builder.End();
        return this;
    }

    /// <summary>
    /// Adds a sibling expression next to the current node.
    /// </summary>
    public AstExpressionBuilder SiblingExpression<TData>(AstKind kind, in TData data)
    {
        if (_current.IsNull)
        {
            throw new InvalidOperationException("Cannot add a sibling without a current node.");
        }

        var handle = _builder.AddSiblingAfter(_current.Ptr, kind);
        _write.AssignNode(handle.Ptr, ref _data, kind, data);
        _current = handle;

        return this;
    }

    /// <summary>
    /// Adds a child expression to the current node without changing scope.
    /// </summary>
    public AstExpressionBuilder ChildExpression<TData>(AstKind kind, in TData data)
    {
        var handle = _builder.AddChild(kind);
        _write.AssignNode(handle.Ptr, ref _data, kind, data);

        _current = handle;

        return this;
    }

    public AstHandle Current => _current;
}
