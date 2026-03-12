using System.Runtime.InteropServices;

namespace JsonPath.Parser;

/// <summary>
/// Indicates which slice parameters were specified in a JsonPath expression.
/// </summary>
[Flags]
public enum SliceFlags : byte
{
    /// <summary>
    /// No parameters were explicitly provided.
    /// </summary>
    None = 0,

    /// <summary>
    /// A start offset is available.
    /// </summary>
    HasStart = 1,

    /// <summary>
    /// An end offset is available.
    /// </summary>
    HasEnd = 2,

    /// <summary>
    /// A step value is available.
    /// </summary>
    HasStep = 4
}

/// <summary>
/// Describes the bounds and stepping information for a JsonPath array slice.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SliceData
{
    /// <summary>
    /// Gets or sets the inclusive starting index when <see cref="HasStart"/> is <see langword="true"/>.
    /// </summary>
    public int Start;

    /// <summary>
    /// Gets or sets the exclusive ending index when <see cref="HasEnd"/> is <see langword="true"/>.
    /// </summary>
    public int End;

    /// <summary>
    /// Gets or sets the traversal step when <see cref="HasStep"/> is <see langword="true"/>.
    /// </summary>
    public int Step;

    /// <summary>
    /// Gets or sets the set of flags describing which members were explicitly specified.
    /// </summary>
    public SliceFlags Flags;

    /// <summary>
    /// Gets a value indicating whether a start index is available.
    /// </summary>
    public readonly bool HasStart => (Flags & SliceFlags.HasStart) != 0;

    /// <summary>
    /// Gets a value indicating whether an end index is available.
    /// </summary>
    public readonly bool HasEnd => (Flags & SliceFlags.HasEnd) != 0;

    /// <summary>
    /// Gets a value indicating whether a step value is available.
    /// </summary>
    public readonly bool HasStep => (Flags & SliceFlags.HasStep) != 0;

    /// <summary>
    /// Returns a canonical textual representation of the slice components.
    /// </summary>
    /// <returns>A string representation of the slice.</returns>
    public readonly override string ToString()
    {
        var s = HasStart ? Start.ToString() : string.Empty;
        var e = HasEnd ? End.ToString() : string.Empty;
        var st = HasStep ? Step.ToString() : string.Empty;
        return $"[{s}:{e}:{st}]";
    }
}