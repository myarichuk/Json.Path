using JsonPath.Parser.Helpers;

namespace JsonPath.Parser;

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

    public AstHandle AddSibling(AstKind kind)
    {
        var current = _ptrStack.Peek();
        var sibling = AllocNode(kind);
        current->NextSibling = sibling;
        _ptrStack.Push(sibling); // move cursor
        return new AstHandle(sibling);
    }
}

public readonly unsafe ref struct AstHandle(AstNode* ptr)
{
    public readonly AstNode* Ptr = ptr;

    public AstHandle Child =>
        Ptr->NextChild == null ?
            default : new AstHandle(Ptr->NextChild);

    public AstHandle Sibling =>
        Ptr->NextSibling == null ?
            default : new AstHandle(Ptr->NextSibling);

    public uint DataIndex => Ptr->DataIndex;

    public AstKind Kind => Ptr->Kind;

    public bool IsNull => Ptr == null;

    /// <inheritdoc />
    public override string ToString() =>
        IsNull ? "<null>" : Kind.ToString();
}