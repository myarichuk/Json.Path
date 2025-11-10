using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JsonPath.Parser.Helpers;
using JsonPath.Parser.Lexer;

namespace JsonPath.Parser;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct FunctionCallData
{
    public readonly ArenaString Name;
    public readonly ArenaList<FunctionArgument>* Args;

    private FunctionCallData(ArenaString name, ArenaList<FunctionArgument>* args)
    {
        Name = name;
        Args = args;
    }

    public static FunctionCallData From(in ReadOnlySpan<char> functionName, ArenaAllocator allocator)
    {
        var argListPtr = (ArenaList<FunctionArgument>*)allocator.Alloc((nuint)Unsafe.SizeOf<ArenaList<FunctionArgument>>());
        *argListPtr = new ArenaList<FunctionArgument>(allocator);
        return new FunctionCallData(
            ArenaString.Clone(functionName, allocator),
            argListPtr);
    }

    public override string ToString() =>
        $"{Name}(args={(Args != null ? Args->Length : 0)})";
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FunctionArgument
{
    public ArenaString ArgName;
    public TokenKind Kind;
    public void* Value;
}