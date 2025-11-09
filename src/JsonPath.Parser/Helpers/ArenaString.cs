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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(ArenaString left, ArenaString right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(ArenaString left, ArenaString right) => !left.Equals(right);

    public static implicit operator ReadOnlySpan<char>(ArenaString s) => s.AsSpan();

    public int Length => len;
    public bool IsEmpty => len == 0 || ptr == null;

    public ReadOnlySpan<char> AsSpan() =>
        ptr == null ? ReadOnlySpan<char>.Empty : new ReadOnlySpan<char>(ptr, len);

    public override string ToString() =>
        ptr == null ? string.Empty : new string(ptr, 0, len);

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
        len == other.Length && AsSpan().SequenceEqual(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ArenaString other) =>
        Equals(other.AsSpan());

    public override bool Equals(object? obj) =>
        obj is ArenaString s && Equals(s);

    public override int GetHashCode() => HashCode.Combine((nint)ptr, len);

  
    public ArenaString Slice(int start, int length)
    {
        #if DEBUG
        // TODO: consider removing DEBUG clause here
        if (start < 0 || length < 0 || start + length > len)
        {
            throw new ArgumentOutOfRangeException();
        }
        #endif

        return new ArenaString(ptr + start, length);
    }
}
