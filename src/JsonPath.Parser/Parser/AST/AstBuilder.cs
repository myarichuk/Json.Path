// using JsonPath.Parser.Helpers;
//
// namespace JsonPath.Parser;
//
// public unsafe struct AstBuilder
// {
//     private readonly ArenaAllocator _allocator;
//     private readonly AstNode* _root;
//     private readonly AstNode* _current;
//     private nuint _sp = 0;
//     private ArenaStack<AstNode> _stack;
//
//     public AstBuilder(in ArenaAllocator allocator)
//     {
//         _allocator = allocator;
//         _stack = new ArenaStack<AstNode>(_allocator);
//
//         _root = (AstNode*)_allocator.Alloc((nuint)sizeof(AstNode));
//         _root->Kind = AstKind.RootIdentifier;
//         _current = _root;
//     }
//
//     private AstNode* AllocNode(AstKind kind)
//     {
//         var node = (AstNode*)_allocator.Alloc((nuint)sizeof(AstNode));
//         node->Kind = kind;
//         node->NextChild = null;
//         node->NextSibling = null;
//         node->DataIndex = 0;
//         return node;
//     }
//
//     public AstHandle AddChild(AstKind kind)
//     {
//         ref var parent = ref _stack.Peek(); // current parent
//         var child = AllocNode(kind);
//
//         if (parent.NextChild == null)
//         {
//             parent.NextChild = child;
//         }
//         else
//         {
//             // find the last sibling
//             var last = parent.NextChild;
//             while (last->NextSibling != null)
//             {
//                 last = last->NextSibling;
//             }
//
//             last->NextSibling = child;
//         }
//
//         return new AstHandle(child);
//     }
//
//     public AstHandle AddSibling(AstKind kind)
//     {
//         ref var current = ref _stack.Peek();
//         var sibling = AllocNode(kind);
//         current.NextSibling = sibling;
//         _stack.Push(*sibling); // move cursor
//         return new AstHandle(sibling);
//     }
//
//     // public ChildBuilderScope PushChild(AstKind kind)
//     // {
//     //     var node = AddChild(kind);
//     //     Push(node.Ptr);
//     //     return new ChildBuilderScope(this);
//     // }
// }
//
// public readonly unsafe ref struct AstHandle(AstNode* ptr)
// {
//     public readonly AstNode* Ptr = ptr;
//
//     public AstHandle Child =>
//         Ptr->NextChild == null ?
//             default : new AstHandle(Ptr->NextChild);
//
//     public AstHandle Sibling =>
//         Ptr->NextSibling == null ?
//             default : new AstHandle(Ptr->NextSibling);
//
//     public uint DataIndex => Ptr->DataIndex;
//
//     public AstKind Kind => Ptr->Kind;
//
//     public bool IsNull => Ptr == null;
//
//     /// <inheritdoc />
//     public override string ToString() =>
//         IsNull ? "<null>" : Kind.ToString();
// }
//
