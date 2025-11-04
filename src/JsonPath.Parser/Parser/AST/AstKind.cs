#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1602

namespace JsonPath.Parser;

/// <summary>
/// AST kinds for JSONPath (RFC 9535)
/// </summary>
public enum AstKind : byte
{
    Unknown, // precaution

    // root and identifier
    RootIdentifier,        // "$" (the root node) :contentReference[oaicite:3]{index=3}
    CurrentNodeIdentifier, // "@" (in filter context) :contentReference[oaicite:4]{index=4}

    // segments
    ChildSegment,          // .name or ['name'] etc — child selection of children only :contentReference[oaicite:5]{index=5}
    DescendantSegment,     // ..[<selectors>] — selects descendants recursively :contentReference[oaicite:6]{index=6}

    // selectors (inside segments)
    NameSelector,          // selects named member from object :contentReference[oaicite:7]{index=7}
    WildcardSelector,      // * wildcard — selects all children :contentReference[oaicite:8]{index=8}
    IndexSelector,         // numeric index into array :contentReference[oaicite:9]{index=9}
    SliceSelector,         // start:end:step array slice :contentReference[oaicite:10]{index=10}
    FilterSelector,        // ?<expr> filter selector :contentReference[oaicite:11]{index=11}
    UnionSelector,         // A union of selectors (e.g., ['a','b',3])

    // expressions inside filters (TODO: make this comprehensive)
    FilterExpression,      // logical/comparison expression inside a filter
    FunctionCall,          // function invocation inside filter (e.g., length(@)) — per spec "function extensions" :contentReference[oaicite:12]{index=12}
    LiteralValue,          // literal (string, number, boolean, null) inside expressions
    CurrentNodeRef,        // "@" inside filter expressions (refers to current node) :contentReference[oaicite:13]{index=13}

    // value operations
    PropertyAccess,        // for accessing a property of an object (via selector) – might map to NameSelector but kept separate if needed
    ArrayAccess,           // for accessing an element of array by index or slice

    // output / result nodes
    ResultNode,            // representation of a selected value in the AST result

    // internal structural nodes
    Sequence,              // sequence of segments
    SegmentList,           // list of segments
    SelectorList, // list of selectors inside a bracket notation
}
