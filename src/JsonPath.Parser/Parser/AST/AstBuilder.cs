using JsonPath.Parser.Helpers;

namespace JsonPath.Parser;

/// <summary>
/// Builds <see cref="AstNode"/> hierarchies using arena-backed allocations.
/// </summary>
public unsafe ref struct AstBuilder
{
    private readonly ArenaAllocator _allocator;
    private readonly AstNode* _root;
    private ArenaPtrStack<AstNode> _stack;

    public AstBuilder(in ArenaAllocator allocator)
    {
        _allocator = allocator;
        _stack = new ArenaPtrStack<AstNode>(_allocator);

        _root = AllocNode(AstKind.RootIdentifier);

        // Root becomes the initial scope
        _stack.Push(_root);
    }

    public AstNode* Root => _root;

    private AstNode* AllocNode(AstKind kind)
    {
        var node = (AstNode*)_allocator.Alloc((nuint)sizeof(AstNode));
        node->Kind = kind;
        node->NextChild = null;
        node->NextSibling = null;
        node->DataIndex = 0;
        return node;
    }

    /// <summary>
    /// Begin a nested AST node: allocate, attach to the current parent, and descend.
    /// </summary>
    public AstHandle Begin(AstKind kind)
    {
        var parent = _stack.Peek();
        var node = AllocNode(kind);

        AttachChild(parent, node);

        _stack.Push(node);
        return new AstHandle(node);
    }

    /// <summary>
    /// Ends the current node scope.
    /// </summary>
    /// <returns>
    /// A handle to the node whose scope was closed, or the root handle when already at the root scope.
    /// </returns>
    public AstHandle End()
    {
        if (_stack.Count <= 1)
        {
            return new AstHandle(_stack.Peek());
        }

        var node = _stack.Pop();
        return new AstHandle(node);
    }

    /// <summary>
    /// Add a child node to the current scope (does not change scope).
    /// </summary>
    public AstHandle AddChild(AstKind kind)
    {
        var parent = _stack.Peek();
        var node = AllocNode(kind);

        AttachChild(parent, node);
        return new AstHandle(node);
    }

    /// <summary>
    /// Add a sibling next to the current node (does not change scope).
    /// </summary>
    public AstHandle AddSibling(AstKind kind)
    {
        var current = _stack.Peek();
        return AddSiblingAfter(current, kind);
    }

    /// <summary>
    /// Adds a sibling relative to the supplied node.
    /// </summary>
    public AstHandle AddSiblingAfter(AstNode* current, AstKind kind)
    {
        var sib = AllocNode(kind);

        // Insert after current node
        sib->NextSibling = current->NextSibling;
        current->NextSibling = sib;

        return new AstHandle(sib);
    }

    /// <summary>
    /// Add a sibling next to the specified node handle.
    /// </summary>
    public AstHandle AddSibling(AstHandle node, AstKind kind)
    {
        if (node.IsNull)
        {
            return AddChild(kind);
        }

        var sib = AllocNode(kind);
        sib->NextSibling = node.Ptr->NextSibling;
        node.Ptr->NextSibling = sib;

        return new AstHandle(sib);
    }

    /// <summary>
    /// Attaches a child node by walking to the last sibling.
    /// </summary>
    private static void AttachChild(AstNode* parent, AstNode* child)
    {
        if (parent->NextChild == null)
        {
            parent->NextChild = child;
            return;
        }

        var last = parent->NextChild;
        while (last->NextSibling != null)
        {
            last = last->NextSibling;
        }

        last->NextSibling = child;
    }
}

/// <summary>
/// Lightweight reference wrapper around an <see cref="AstNode"/> pointer.
/// </summary>
public readonly unsafe ref struct AstHandle(AstNode* ptr)
{
    /// <summary>
    /// Gets the raw pointer represented by the handle.
    /// </summary>
    public readonly AstNode* Ptr = ptr;

    /// <summary>
    /// Gets a handle to the first child node, or the default value when none exist.
    /// </summary>
    public AstHandle Child =>
        Ptr->NextChild == null ?
            default : new AstHandle(Ptr->NextChild);

    /// <summary>
    /// Gets a handle to the next sibling node, or the default value when none exist.
    /// </summary>
    public AstHandle Sibling =>
        Ptr->NextSibling == null ?
            default : new AstHandle(Ptr->NextSibling);

    /// <summary>
    /// Gets the data index associated with the underlying node.
    /// </summary>
    public uint DataIndex => Ptr->DataIndex;

    /// <summary>
    /// Gets the AST kind represented by the node.
    /// </summary>
    public AstKind Kind => Ptr->Kind;

    /// <summary>
    /// Gets a value indicating whether the handle is empty.
    /// </summary>
    public bool IsNull => Ptr == null;

    /// <inheritdoc />
    public override string ToString() =>
        IsNull ? "<null>" : Kind.ToString();
}