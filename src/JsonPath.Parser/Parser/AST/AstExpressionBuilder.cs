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
    public AstExpressionBuilder BeginExpression(AstKind kind)
    {
        // allocate + push into the AST
        var handle = _builder.Begin(kind);

        // attach data to that node
        _write.AssignNode(handle.Ptr, kind);

        _current = handle;
        return this;
    }

    /// <summary>
    /// Ends the current nested expression scope.
    /// </summary>
    public AstExpressionBuilder EndExpression()
    {
        var ended = _builder.End();
        _current = ended;
        return this;
    }

    /// <summary>
    /// Adds a sibling expression next to the current node.
    /// </summary>
    public AstExpressionBuilder SiblingExpression(AstKind kind)
    {
        var handle = _builder.AddSibling(_current, kind);
        _write.AssignNode(handle.Ptr, kind);
        _current = handle;

        return this;
    }

    /// <summary>
    /// Adds a child expression to the current node without changing scope.
    /// </summary>
    public AstExpressionBuilder ChildExpression(AstKind kind)
    {
        var handle = _builder.AddChild(kind);
        _write.AssignNode(handle.Ptr, kind);

        _current = handle;

        return this;
    }

    public AstExpressionBuilder BeginNameSelector(string name)
    {
        var handle = _builder.Begin(AstKind.NameSelector);
        _write.AssignNameSelector(handle.Ptr, ref _data, name);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder BeginNameSelector(in ArenaString name)
    {
        var handle = _builder.Begin(AstKind.NameSelector);
        _write.AssignNameSelector(handle.Ptr, ref _data, name);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder ChildNameSelector(string name)
    {
        var handle = _builder.AddChild(AstKind.NameSelector);
        _write.AssignNameSelector(handle.Ptr, ref _data, name);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder ChildNameSelector(in ArenaString name)
    {
        var handle = _builder.AddChild(AstKind.NameSelector);
        _write.AssignNameSelector(handle.Ptr, ref _data, name);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder SiblingNameSelector(string name)
    {
        var handle = _builder.AddSibling(_current, AstKind.NameSelector);
        _write.AssignNameSelector(handle.Ptr, ref _data, name);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder SiblingNameSelector(in ArenaString name)
    {
        var handle = _builder.AddSibling(_current, AstKind.NameSelector);
        _write.AssignNameSelector(handle.Ptr, ref _data, name);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder BeginLiteralValue(in LiteralData literal)
    {
        var handle = _builder.Begin(AstKind.LiteralValue);
        _write.AssignLiteral(handle.Ptr, ref _data, literal);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder ChildLiteralValue(in LiteralData literal)
    {
        var handle = _builder.AddChild(AstKind.LiteralValue);
        _write.AssignLiteral(handle.Ptr, ref _data, literal);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder SiblingLiteralValue(in LiteralData literal)
    {
        var handle = _builder.AddSibling(_current, AstKind.LiteralValue);
        _write.AssignLiteral(handle.Ptr, ref _data, literal);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder BeginIndexSelector(long index)
    {
        var handle = _builder.Begin(AstKind.IndexSelector);
        _write.AssignIndexSelector(handle.Ptr, ref _data, index);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder ChildIndexSelector(long index)
    {
        var handle = _builder.AddChild(AstKind.IndexSelector);
        _write.AssignIndexSelector(handle.Ptr, ref _data, index);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder SiblingIndexSelector(long index)
    {
        var handle = _builder.AddSibling(_current, AstKind.IndexSelector);
        _write.AssignIndexSelector(handle.Ptr, ref _data, index);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder BeginSliceSelector(in SliceData slice)
    {
        var handle = _builder.Begin(AstKind.SliceSelector);
        _write.AssignSliceSelector(handle.Ptr, ref _data, slice);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder ChildSliceSelector(in SliceData slice)
    {
        var handle = _builder.AddChild(AstKind.SliceSelector);
        _write.AssignSliceSelector(handle.Ptr, ref _data, slice);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder SiblingSliceSelector(in SliceData slice)
    {
        var handle = _builder.AddSibling(_current, AstKind.SliceSelector);
        _write.AssignSliceSelector(handle.Ptr, ref _data, slice);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder BeginFunctionCall(in FunctionCallData fn)
    {
        var handle = _builder.Begin(AstKind.FunctionCall);
        _write.AssignFunctionCall(handle.Ptr, ref _data, fn);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder ChildFunctionCall(in FunctionCallData fn)
    {
        var handle = _builder.AddChild(AstKind.FunctionCall);
        _write.AssignFunctionCall(handle.Ptr, ref _data, fn);
        _current = handle;
        return this;
    }

    public AstExpressionBuilder SiblingFunctionCall(in FunctionCallData fn)
    {
        var handle = _builder.AddSibling(_current, AstKind.FunctionCall);
        _write.AssignFunctionCall(handle.Ptr, ref _data, fn);
        _current = handle;
        return this;
    }

    public AstHandle Current => _current;
}
