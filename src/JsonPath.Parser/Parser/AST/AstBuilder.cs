using JsonPath.Parser.Helpers;

namespace JsonPath.Parser;

/// <summary>
/// Builds <see cref="AstNode"/> hierarchies using arena-backed allocations.
/// </summary>
public unsafe ref struct AstBuilder
{
    private readonly ArenaAllocator _allocator;
    private readonly AstNode* _root;
    private ArenaPtrStack<AstNode> _ptrStack;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AstBuilder"/> struct.
    /// </summary>
    /// <param name="allocator">bump-allocator instance</param>
    public AstBuilder(in ArenaAllocator allocator)
    {
        _allocator = allocator;
        _ptrStack = new ArenaPtrStack<AstNode>(_allocator);

        _root = AllocNode(AstKind.RootIdentifier);
    }

    /// <summary>
    /// Gets the root node of the constructed AST.
    /// </summary>
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
    /// Adds a child node under the current parent and returns a handle to it.
    /// </summary>
    /// <typeparam name="TData">Unused generic type parameter kept for parity with legacy API expectations.</typeparam>
    /// <param name="kind">The AST kind of the child node.</param>
    /// <returns>An <see cref="AstHandle"/> pointing to the new node.</returns>
    public AstHandle AddChild<TData>(AstKind kind)
        where TData : unmanaged
    {
        var parent = _ptrStack.Peek(); // current parent
        var child = AllocNode(kind);

        if (parent->NextChild == null)
        {
            parent->NextChild = child;
        }
        else
        {
            // find the last sibling
            var last = parent->NextChild;
            while (last->NextSibling != null)
            {
                last = last->NextSibling;
            }

            last->NextSibling = child;
        }

        return new AstHandle(child);
    }

    /// <summary>
    /// Adds a sibling node next to the current node and advances the cursor.
    /// </summary>
    /// <param name="kind">The AST kind of the sibling node.</param>
    /// <returns>An <see cref="AstHandle"/> pointing to the new node.</returns>
    public AstHandle AddSibling(AstKind kind)
    {
        var current = _ptrStack.Peek();
        var sibling = AllocNode(kind);
        current->NextSibling = sibling;
        _ptrStack.Push(sibling); // move cursor
        return new AstHandle(sibling);
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