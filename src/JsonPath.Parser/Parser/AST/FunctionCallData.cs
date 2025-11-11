using System.Runtime.InteropServices;
using JsonPath.Parser.Helpers;
using JsonPath.Parser.Lexer;

namespace JsonPath.Parser;

[StructLayout(LayoutKind.Sequential)]
public struct FunctionCallData
{
    public readonly ArenaString Name;
    public uint ArgumentStartOffset;
    public ushort ArgumentCount;

    private FunctionCallData(ArenaString name)
    {
        Name = name;
        ArgumentStartOffset = 0;
        ArgumentCount = 0;
    }

    public static FunctionCallData From(in ReadOnlySpan<char> functionName, ArenaAllocator allocator)
    {
        return new FunctionCallData(ArenaString.Clone(functionName, allocator));
    }

    public override string ToString() =>
        $"{Name}(args={ArgumentCount})";
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FunctionArgument
{
    public ArenaString ArgName;
    public TokenKind Kind;
    public void* Value;
}