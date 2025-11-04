using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace JsonPath.Parser;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct AstNode
{
    public AstKind Kind;
    public AstNode* NextChild;
    public AstNode* NextSibling;

    public uint DataIndex;
}