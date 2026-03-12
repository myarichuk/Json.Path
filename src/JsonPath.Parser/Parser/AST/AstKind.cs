#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1602

namespace JsonPath.Parser;

/// <summary>
/// AST kinds for JSONPath (RFC 9535).
/// The enum values correspond to identifiers, segments, selectors, and filter expression constructs defined in the specification.
/// </summary>
public enum AstKind : byte
{
    Unknown, // precaution

    // root and identifier
    RootIdentifier,        // "$" — root identifier
    CurrentNodeIdentifier, // "@" — current item identifier in filter context

    // segments
    ChildSegment,          // Child segment (dot or bracket notation) selecting direct children
    DescendantSegment,     // Descendant segment (recursive descent) selecting nested descendants

    // selectors (inside segments)
    NameSelector,          // Name selector for object members
    WildcardSelector,      // Wildcard selector (*) returning all children
    IndexSelector,         // Array index selector (single element)
    SliceSelector,         // Array slice selector (start:end:step)
    FilterSelector,        // Filter selector (?(<expression>)) returning nodes that satisfy the predicate
    UnionSelector,         // A union of selectors (e.g., ['a','b',3])

    // expressions inside filters (TODO: make this comprehensive)
    FilterExpression,      // Logical or comparison expression within a filter
    FunctionCall,          // Function extension invocation inside expressions (e.g., length(@))
    LiteralValue,          // Literal value (string, number, boolean, or null) inside expressions
    CurrentNodeRef,        // Current node reference "@" within filter expressions

    // value operations
    PropertyAccess,        // Property access via selector (separate from NameSelector for clarity)
    ArrayAccess,           // Array element access by index or slice

    // output / result nodes
    ResultNode,            // representation of a selected value in the AST result

    // internal structural nodes
    Sequence,              // sequence of segments
    SegmentList,           // list of segments
    SelectorList, // list of selectors inside a bracket notation
}
