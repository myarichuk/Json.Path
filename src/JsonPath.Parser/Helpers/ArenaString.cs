using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// ReSharper disable MemberCanBePrivate.Global

namespace JsonPath.Parser.Helpers;


/// <summary>
/// A non-owning view of UTF-16 text stored in unmanaged (arena) memory.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct ArenaString(char* ptr, int len)
{
    public int Length => len;

    public bool IsEmpty => len == 0 || ptr == null;

    // ReSharper disable once MemberCanBePrivate.Global
    public ReadOnlySpan<char> AsSpan() => new(ptr, len);

    /// <summary>
    /// Allocate and fetch managed string representation
    /// </summary>
    /// <returns>copy of the content</returns>
    public override string ToString() =>
        ptr is null ? string.Empty : new string(ptr, 0, len);

    /// <summary>
    /// Allocates unmanaged memory in the given arena and copies the text into it.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArenaString Clone(ReadOnlySpan<char> src, ArenaAllocator arena)
    {
        if (src.IsEmpty)
        {
            return default;
        }

        var bytes = (nuint)(src.Length * sizeof(char));
        var dest = (char*)arena.Alloc(bytes, align: (nuint)UnsafeHelpers.AlignOf<char>());
        src.CopyTo(new Span<char>(dest, src.Length));
        return new ArenaString(dest, src.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ReadOnlySpan<char> other) =>
        AsSpan().SequenceEqual(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ArenaString other) =>
        Equals(other.AsSpan());

    public ArenaString Slice(int start, int length) =>
        new(ptr + start, length);

    public static implicit operator ReadOnlySpan<char>(ArenaString s) => s.AsSpan();
}
