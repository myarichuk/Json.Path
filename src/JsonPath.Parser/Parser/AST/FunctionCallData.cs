using System.Runtime.InteropServices;
using JsonPath.Parser.Helpers;
using JsonPath.Parser.Lexer;

namespace JsonPath.Parser;

/// <summary>
/// Represents a function invocation captured during JsonPath parsing.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct FunctionCallData
{
    /// <summary>
    /// Gets the name of the function being invoked.
    /// </summary>
    public readonly ArenaString Name;

    /// <summary>
    /// Gets or sets the offset into the argument table where this function's parameters begin.
    /// </summary>
    public uint ArgumentStartOffset;

    /// <summary>
    /// Gets or sets the number of arguments supplied to the function.
    /// </summary>
    public ushort ArgumentCount;

    private FunctionCallData(ArenaString name)
    {
        Name = name;
        ArgumentStartOffset = 0;
        ArgumentCount = 0;
    }

    /// <summary>
    /// Creates a <see cref="FunctionCallData"/> value from the provided function name.
    /// </summary>
    /// <param name="functionName">The source name span.</param>
    /// <param name="allocator">Allocator used to persist the name.</param>
    /// <returns>A populated function descriptor.</returns>
    public static FunctionCallData From(in ReadOnlySpan<char> functionName, ArenaAllocator allocator)
    {
        return new FunctionCallData(ArenaString.Clone(functionName, allocator));
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{Name}(args={ArgumentCount})";
}

/// <summary>
/// Represents a single argument to a JsonPath function call.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct FunctionArgument
{
    /// <summary>
    /// Gets or sets the argument name when using named parameters.
    /// </summary>
    public ArenaString ArgName;

    /// <summary>
    /// Gets or sets the token kind describing the argument payload type.
    /// </summary>
    public TokenKind Kind;

    /// <summary>
    /// Gets or sets a pointer to the value stored for the argument.
    /// </summary>
    public void* Value;
}