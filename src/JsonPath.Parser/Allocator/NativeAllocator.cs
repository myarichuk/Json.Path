using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JsonPath.Parser.Allocator;

/// <summary>
/// Specifies which low-level allocator to use.
/// </summary>
public enum NativeAllocatorBackend
{
    /// <summary>
    /// Use .NET's <see cref="System.Runtime.InteropServices.NativeMemory"/> API.
    /// </summary>
    DotNetUnmanaged,

    /// <summary>
    /// Use platform-native virtual memory APIs (VirtualAlloc / mmap).
    /// </summary>
    PlatformInvoke,
}

/// <summary>
/// Specifies whether the allocated memory should be protected (read-only / no-access).
/// Used for debugging leaks with guard pages.
/// </summary>
public enum MemoryProtectionMode
{
    None,
    ReadOnly,
    NoAccess,
}

/// <summary>
/// Allocator that provides raw, unmanaged memory using either .NET NativeMemory
/// or platform-native VirtualAlloc/mmap calls.
/// </summary>
public static unsafe class NativeAllocator
{
    private const ulong MagicValue = 0xDEADC0DECAFEBEEFUL;
    private const ulong FreedValue = 0xFEEDF00DDEADBEAFL;

    [StructLayout(LayoutKind.Sequential)]
    private struct AllocationHeader
    {
        public ulong Magic;
        public ulong Size;
    }

    private static readonly nuint HeaderSize = (nuint)sizeof(AllocationHeader);

    private static readonly Dictionary<nint, nuint> _active = new();

    public static void* Alloc(
        nuint size,
        NativeAllocatorBackend backend = NativeAllocatorBackend.PlatformInvoke,
        MemoryProtectionMode protection = MemoryProtectionMode.None)
    {
        if (size == 0)
        {
            return null;
        }

        var total = size + HeaderSize;
        void* rawPtr;

        if (backend == NativeAllocatorBackend.DotNetUnmanaged)
        {
            rawPtr = NativeMemory.Alloc(total);
        }
        else if (OperatingSystem.IsWindows())
        {
            rawPtr = (void*)VirtualAlloc(0, total, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
        }
        else
        {
            rawPtr = (void*)mmap(IntPtr.Zero, total, PROT_READ | PROT_WRITE,
                MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
        }

        var hdr = (AllocationHeader*)rawPtr;
        hdr->Magic = MagicValue;
        hdr->Size = size;
        var userPtr = (byte*)rawPtr + HeaderSize;

        lock (_active)
        {
            _active.Add((nint)userPtr, total);
        }

        if (protection != MemoryProtectionMode.None)
        {
            ApplyProtection(userPtr, size, protection);
        }

        return userPtr;
    }

    public static void Free(
        void* userPtr,
        NativeAllocatorBackend backend = NativeAllocatorBackend.PlatformInvoke)
    {
        if (userPtr == null)
        {
            return;
        }

        nint key = (nint)userPtr;
        nuint total;

        lock (_active)
        {
            if (!_active.Remove(key, out total))
            {
                throw new InvalidOperationException("Double free or foreign pointer detected");
            }
        }

        var hdr = (AllocationHeader*)((byte*)userPtr - HeaderSize);
        if (hdr->Magic == FreedValue)
        {
            throw new InvalidOperationException("Double free or foreign pointer detected");
        }

        hdr->Magic = FreedValue;

        void* rawPtr = hdr;

        if (backend == NativeAllocatorBackend.DotNetUnmanaged)
        {
            NativeMemory.Free(rawPtr);
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            if (!VirtualFree((nint)rawPtr, 0, MEM_RELEASE))
            {
                ThrowLastError("VirtualFree failed");
            }
        }
        else
        {
            if (munmap((IntPtr)rawPtr, total) != 0)
            {
                ThrowLastError("munmap failed");
            }
        }
    }

    public static void ApplyProtection(void* ptr, nuint size, MemoryProtectionMode mode)
    {
        if (ptr == null || size == 0)
        {
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            uint prot = mode switch
            {
                MemoryProtectionMode.ReadOnly => PAGE_READONLY,
                MemoryProtectionMode.NoAccess => PAGE_NOACCESS,
                _ => PAGE_READWRITE,
            };

            if (!VirtualProtect((nint)ptr, size, prot, out _))
            {
                ThrowLastError("VirtualProtect failed");
            }
        }
        else
        {
            int prot = mode switch
            {
                MemoryProtectionMode.ReadOnly => PROT_READ,
                MemoryProtectionMode.NoAccess => PROT_NONE,
                _ => PROT_READ | PROT_WRITE,
            };

            if (mprotect((IntPtr)ptr, size, prot) != 0)
            {
                ThrowLastError("mprotect failed");
            }
        }
    }

    #region PInvoke

    private const string Kernel32 = "kernel32.dll";
    private const uint MEM_COMMIT = 0x1000;
    private const uint MEM_RESERVE = 0x2000;
    private const uint MEM_RELEASE = 0x8000;
    private const uint PAGE_READWRITE = 0x04;
    private const uint PAGE_READONLY = 0x02;
    private const uint PAGE_NOACCESS = 0x01;

    [DllImport(Kernel32, SetLastError = true)]
    private static extern nint VirtualAlloc(nint lpAddress, nuint dwSize, uint flAllocationType, uint flProtect);

    [DllImport(Kernel32, SetLastError = true)]
    private static extern bool VirtualFree(nint lpAddress, nuint dwSize, uint dwFreeType);

    [DllImport(Kernel32, SetLastError = true)]
    private static extern bool VirtualProtect(nint lpAddress, nuint dwSize, uint flNewProtect, out uint lpflOldProtect);

    [DllImport(Kernel32, SetLastError = true)]
    private static extern nuint VirtualQuery(nint lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, nuint dwLength);

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORY_BASIC_INFORMATION
    {
        public nint BaseAddress;
        public nint AllocationBase;
        public uint AllocationProtect;
        public nuint RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    private const int PROT_NONE = 0;
    private const int PROT_READ = 1;
    private const int PROT_WRITE = 2;
    private const int MAP_PRIVATE = 2;
    private const int MAP_ANONYMOUS = 0x20;

    [DllImport("libc", SetLastError = true)]
    private static extern IntPtr mmap(IntPtr addr, nuint length, int prot, int flags, int fd, long offset);

    [DllImport("libc", SetLastError = true)]
    private static extern int munmap(IntPtr addr, nuint length);

    [DllImport("libc", SetLastError = true)]
    private static extern int mprotect(IntPtr addr, nuint len, int prot);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowLastError(string msg)
    {
        var err = Marshal.GetLastWin32Error();
        throw new InvalidOperationException($"{msg} (errno {err})");
    }

    #endregion
}
