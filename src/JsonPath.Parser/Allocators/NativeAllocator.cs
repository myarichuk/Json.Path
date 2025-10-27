using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if DEBUG
using System.Collections.Concurrent;
#endif

namespace JsonPath.Parser.Allocators;

public enum NativeAllocatorBackend
{
    DotNetUnmanaged,
    PlatformInvoke,
}

public enum MemoryProtectionMode
{
    None,
    ReadOnly,
    NoAccess,
}

public static unsafe class NativeAllocator
{
    private const ulong MagicValue = 0xDEADC0DECAFEBEEFUL;
    private const ulong FreedValue = 0xFEEDF00DDEADBEAFL;

    private static readonly nuint HeaderSize = (nuint)sizeof(AllocationHeader);
    private static readonly nuint PageSize = (nuint)Environment.SystemPageSize;

    // ConcurrentDictionary ensures async/thread safety
#if DEBUG
    private static readonly ConcurrentDictionary<nint, AllocationInfo> _active = new();

    private readonly struct AllocationInfo
    {
        public AllocationInfo(nint rawPtr, nuint reservedSize, nuint guardPrefix, NativeAllocatorBackend backend)
        {
            RawPtr = rawPtr;
            ReservedSize = reservedSize;
            GuardPrefix = guardPrefix;
            Backend = backend;
        }

        public nint RawPtr { get; }

        public nuint ReservedSize { get; }

        public nuint GuardPrefix { get; }

        public NativeAllocatorBackend Backend { get; }

        public AllocationHeader* Header
            => (AllocationHeader*)((byte*)RawPtr + GuardPrefix);
    }
#endif

    [StructLayout(LayoutKind.Sequential)]
    private struct AllocationHeader
    {
        public ulong Magic;
        public nuint Size;
        public nuint ReservedSize;
        public nuint GuardPrefix;
        public nuint GuardSuffix;
        public NativeAllocatorBackend Backend;
    }

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
        var alignedTotal = total;
        nuint guardPrefix = 0;
        nuint guardSuffix = 0;


        if (backend is NativeAllocatorBackend.PlatformInvoke)
        {
            alignedTotal = AlignUp(total, PageSize);
#if DEBUG
            guardPrefix = PageSize;
            guardSuffix = PageSize;
            alignedTotal += guardPrefix + guardSuffix;
#endif
        }

        void* rawPtr = backend switch
        {
            NativeAllocatorBackend.DotNetUnmanaged => NativeMemory.Alloc(total),
            _ when OperatingSystem.IsWindows()
                => (void*)Native.VirtualAlloc(0, alignedTotal, Native.MEM_RESERVE | Native.MEM_COMMIT, Native.PAGE_READWRITE),
            _ => (void*)Native.mmap(IntPtr.Zero, alignedTotal, Native.PROT_READ | Native.PROT_WRITE,
                                    Native.MAP_PRIVATE | Native.MAP_ANONYMOUS, -1, 0),
        };

        if (backend is NativeAllocatorBackend.PlatformInvoke && IsMmapFailure(rawPtr))
        {
            rawPtr = null;
        }

        if (rawPtr is null)
        {
            throw new OutOfMemoryException("Native allocation failed");
        }

        var headerPtr = (AllocationHeader*)((byte*)rawPtr + guardPrefix);
        var hdr = headerPtr;
        hdr->Magic = MagicValue;
        hdr->Size = size;
        hdr->ReservedSize = alignedTotal;
        hdr->GuardPrefix = guardPrefix;
        hdr->GuardSuffix = guardSuffix;
        hdr->Backend = backend;

        var userPtr = (byte*)headerPtr + HeaderSize;

        if (protection != MemoryProtectionMode.None &&
            backend is NativeAllocatorBackend.PlatformInvoke)
        {
            ApplyProtection(userPtr, size, protection);
        }

#if DEBUG
        if (backend is NativeAllocatorBackend.PlatformInvoke)
        {
            ApplyGuard(rawPtr, guardPrefix, guardSuffix, alignedTotal);
        }
#endif

#if DEBUG
        var info = new AllocationInfo((nint)rawPtr, alignedTotal, guardPrefix, backend);
        _active[(nint)userPtr] = info;
#endif

        return userPtr;
    }

    public static void Free(void* userPtr,
                            NativeAllocatorBackend backend = NativeAllocatorBackend.PlatformInvoke)
    {
        if (userPtr is null)
        {
            return;
        }

        var header = (AllocationHeader*)((byte*)userPtr - HeaderSize);

#if DEBUG
        var key = (nint)userPtr;

        if (!_active.TryRemove(key, out var info))
        {
            throw new InvalidOperationException("Double free or foreign pointer detected.");
        }

        var rawPtr = info.RawPtr;
        var reservedSize = info.ReservedSize;
        var expectedBackend = info.Backend;
#else
        nint rawPtr = 0;
        nuint reservedSize = 0;
        NativeAllocatorBackend expectedBackend = backend;

        try
        {
            var guardPrefix = header->GuardPrefix;
            rawPtr = (nint)((byte*)header - guardPrefix);
            reservedSize = header->ReservedSize;
            expectedBackend = header->Backend;
        }
        catch (AccessViolationException)
        {
            throw new InvalidOperationException("Foreign pointer detected.");
        }
#endif

        try
        {
            if (header->Magic != MagicValue)
            {
                throw new InvalidOperationException("Foreign pointer detected.");
            }

            header->Magic = FreedValue;
        }
        catch (AccessViolationException)
        {
            throw new InvalidOperationException("Foreign pointer detected.");
        }

        if (backend != expectedBackend)
        {
            throw new InvalidOperationException("Allocator backend mismatch.");
        }

        switch (expectedBackend)
        {
            case NativeAllocatorBackend.DotNetUnmanaged:
                NativeMemory.Free((void*)rawPtr);
                return;

            case NativeAllocatorBackend.PlatformInvoke:
                if (OperatingSystem.IsWindows())
                {
                    if (!Native.VirtualFree(rawPtr, 0, Native.MEM_RELEASE))
                    {
                        ThrowLastError("VirtualFree failed");
                    }
                }
                else if (Native.munmap((IntPtr)rawPtr, reservedSize) != 0)
                {
                    ThrowLastError("munmap failed");
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(backend));
        }
    }

    private static bool IsMmapFailure(void* ptr)
        => !OperatingSystem.IsWindows() && (nint)ptr == -1;

    private static void AlignToPage(void* ptr, nuint length, out void* alignedPtr, out nuint alignedLength)
    {
        var address = (nuint)ptr;
        var start = AlignDown(address, PageSize);
        var end = AlignUp(address + length, PageSize);
        alignedPtr = (void*)start;
        alignedLength = end - start;
    }

    private static nuint AlignUp(nuint value, nuint alignment)
    {
        if (alignment == 0)
        {
            return value;
        }

        var remainder = value % alignment;
        return remainder == 0 ? value : value + (alignment - remainder);
    }

    private static nuint AlignDown(nuint value, nuint alignment)
    {
        if (alignment == 0)
        {
            return value;
        }

        var remainder = value % alignment;
        return value - remainder;
    }

#if DEBUG
    private static void ApplyGuard(void* basePtr, nuint guardPrefix, nuint guardSuffix, nuint total)
    {
        if (guardPrefix == 0 && guardSuffix == 0)
        {
            return;
        }

        if (guardPrefix != 0)
        {
            ProtectGuard(basePtr, guardPrefix);
        }

        if (guardSuffix != 0)
        {
            var suffixPtr = (byte*)basePtr + total - guardSuffix;
            ProtectGuard(suffixPtr, guardSuffix);
        }
    }

    private static void ProtectGuard(void* ptr, nuint length)
    {
        if (length == 0)
        {
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            if (!Native.VirtualProtect((nint)ptr, length, Native.PAGE_NOACCESS, out _))
            {
                ThrowLastError("VirtualProtect guard failed");
            }
        }
        else if (Native.mprotect((IntPtr)ptr, length, Native.PROT_NONE) != 0)
        {
            ThrowLastError("mprotect guard failed");
        }
    }
#endif

    public static void ApplyProtection(void* ptr, nuint size, MemoryProtectionMode mode)
    {
        if (ptr is null || size == 0)
        {
            return;
        }

        AlignToPage(ptr, size, out var alignedPtr, out var alignedLength);

        var header = (AllocationHeader*)((byte*)ptr - HeaderSize);

        if (header->GuardPrefix != 0 || header->GuardSuffix != 0)
        {
            var start = (nuint)alignedPtr;
            var end = start + alignedLength;
            var basePtr = (nuint)((byte*)header - header->GuardPrefix);
            var userStart = basePtr + header->GuardPrefix;
            var userEnd = basePtr + header->ReservedSize - header->GuardSuffix;

            if (start < userStart)
            {
                var delta = userStart - start;
                if (delta >= alignedLength)
                {
                    return;
                }

                start = userStart;
                alignedPtr = (void*)start;
                alignedLength -= delta;
                end = start + alignedLength;
            }

            if (end > userEnd)
            {
                var delta = end - userEnd;
                if (delta >= alignedLength)
                {
                    return;
                }

                alignedLength -= delta;
                end = userEnd;
            }

            if (alignedLength == 0)
            {
                return;
            }
        }

        if (OperatingSystem.IsWindows())
        {
            var prot = mode switch
            {
                MemoryProtectionMode.ReadOnly => Native.PAGE_READONLY,
                MemoryProtectionMode.NoAccess => Native.PAGE_NOACCESS,
                _ => Native.PAGE_READWRITE,
            };
            if (!Native.VirtualProtect((nint)alignedPtr, alignedLength, prot, out _))
            {
                ThrowLastError("VirtualProtect failed");
            }
        }
        else
        {
            var prot = mode switch
            {
                MemoryProtectionMode.ReadOnly => Native.PROT_READ,
                MemoryProtectionMode.NoAccess => Native.PROT_NONE,
                _ => Native.PROT_READ | Native.PROT_WRITE,
            };
            if (Native.mprotect((IntPtr)alignedPtr, alignedLength, prot) != 0)
            {
                ThrowLastError("mprotect failed");
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowLastError(string msg)
        => throw new InvalidOperationException($"{msg} (errno {Marshal.GetLastWin32Error()})");

#if DEBUG
    public static int ActiveCount => _active.Count;
#endif
}


internal static partial class Native
{
    public const string Kernel32 = "kernel32.dll";

    // Win32 constants
    public const uint MEM_COMMIT = 0x1000, MEM_RESERVE = 0x2000, MEM_RELEASE = 0x8000;
    public const uint PAGE_READWRITE = 0x04, PAGE_READONLY = 0x02, PAGE_NOACCESS = 0x01;

    // POSIX constants
    public const int PROT_NONE = 0, PROT_READ = 1, PROT_WRITE = 2;
    public const int MAP_PRIVATE = 2, MAP_ANONYMOUS = 0x20;

    [LibraryImport(Kernel32, SetLastError = true)]
    public static partial nint VirtualAlloc(nint lpAddress, nuint dwSize, uint flAllocationType, uint flProtect);

    [LibraryImport(Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool VirtualFree(nint lpAddress, nuint dwSize, uint dwFreeType);

    [LibraryImport(Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool VirtualProtect(nint lpAddress, nuint dwSize, uint flNewProtect, out uint lpflOldProtect);

    [LibraryImport(Kernel32, SetLastError = true)]
    public static partial nuint VirtualQuery(nint lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, nuint dwLength);

    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION
    {
        public nint BaseAddress;
        public nint AllocationBase;
        public uint AllocationProtect;
        public nuint RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    [LibraryImport("libc", SetLastError = true)]
    public static partial IntPtr mmap(IntPtr addr, nuint length, int prot, int flags, int fd, long offset);

    [LibraryImport("libc", SetLastError = true)]
    public static partial int munmap(IntPtr addr, nuint length);

    [LibraryImport("libc", SetLastError = true)]
    public static partial int mprotect(IntPtr addr, nuint len, int prot);
}