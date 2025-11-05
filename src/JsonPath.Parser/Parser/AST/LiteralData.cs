using System.Runtime.InteropServices;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser;

public enum LiteralKind : byte
{
    String,
    Number,
    Boolean,
    Null
}

[StructLayout(LayoutKind.Sequential)]
public struct LiteralData
{
    public LiteralKind Kind;
    public double Number;
    public byte Bool;
    public ArenaString String;

    public readonly override string ToString() =>
        Kind switch
        {
            LiteralKind.String => $"\"{String.ToString()}\"",
            LiteralKind.Number => Number.ToString(),
            LiteralKind.Boolean => Bool != 0 ? "true" : "false",
            LiteralKind.Null => "null",
            _ => "<invalid>",
        };
}