using System.Runtime.InteropServices;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser;

[StructLayout(LayoutKind.Sequential)]
public struct FunctionCallData
{
    public ArenaString Name;
    public uint FirstArgIndex; // index into AST node sequence for first argument
    public ushort ArgCount;

    public readonly override string ToString() =>
        $"{Name.ToString()}(args={ArgCount})";
}