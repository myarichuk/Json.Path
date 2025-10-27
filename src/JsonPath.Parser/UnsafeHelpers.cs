using System.Runtime.CompilerServices;

namespace JsonPath.Parser;

public class UnsafeHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AlignOf<T>() where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();

        // assume at least pointer-size alignment (worst case bit over-align)
        return size < IntPtr.Size ? IntPtr.Size : size;
    }
}