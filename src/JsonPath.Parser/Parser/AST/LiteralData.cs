using System.Globalization;
using System.Runtime.InteropServices;
using JsonPath.Parser.Helpers;

namespace JsonPath.Parser;

/// <summary>
/// Identifies the runtime representation used for a literal token.
/// </summary>
public enum LiteralKind : byte
{
    /// <summary>
    /// Literal stores string data.
    /// </summary>
    String,

    /// <summary>
    /// Literal stores a numeric value.
    /// </summary>
    Number,

    /// <summary>
    /// Literal stores a boolean flag.
    /// </summary>
    Boolean,

    /// <summary>
    /// Literal represents the <see langword="null"/> value.
    /// </summary>
    Null
}

/// <summary>
/// Stores a literal token and its associated value in arena-allocated memory.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct LiteralData
{
    /// <summary>
    /// Gets or sets the literal classification.
    /// </summary>
    [FieldOffset(0)] 
    public LiteralKind Kind;

    /// <summary>
    /// Gets or sets the numeric literal value when <see cref="Kind"/> is <see cref="LiteralKind.Number"/>.
    /// </summary>
    [FieldOffset(8)]     
    public double Number;

    /// <summary>
    /// Gets or sets the boolean literal value when <see cref="Kind"/> is <see cref="LiteralKind.Boolean"/>.
    /// </summary>
    [FieldOffset(8)]     
    public byte Bool;

    /// <summary>
    /// Gets or sets the arena-backed string when <see cref="Kind"/> is <see cref="LiteralKind.String"/>.
    /// </summary>
    [FieldOffset(8)]     
    public ArenaString String;

    /// <summary>
    /// Returns a textual representation of the literal value.
    /// </summary>
    /// <returns>String form of the literal respecting the underlying kind.</returns>
    public readonly override string ToString() =>
        Kind switch
        {
            LiteralKind.String => $"\"{String}\"",
            LiteralKind.Number => Number.ToString(CultureInfo.InvariantCulture),
            LiteralKind.Boolean => Bool != 0 ? "true" : "false",
            LiteralKind.Null => "null",
            _ => "<invalid>",
        };
}