using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
    private static readonly Dictionary<nint, nuint> _active = new();

    [StructLayout(LayoutKind.Sequential)]
    private struct AllocationHeader
    {
        public ulong Magic;
        public ulong Size;
    }

    public static void* Alloc(
        nuint size,
        NativeAllocatorBackend backend = NativeAllocatorBackend.PlatformInvoke,
        MemoryProtectionMode protection = MemoryProtectionMode.None)
    {
        if (size == 0) return null;

        var total = size + HeaderSize;
        void* rawPtr = backend switch
        {
            NativeAllocatorBackend.DotNetUnmanaged => NativeMemory.Alloc(total),
            _ when OperatingSystem.IsWindows()
                => (void*)Native.VirtualAlloc(0, total, Native.MEM_RESERVE | Native.MEM_COMMIT, Native.PAGE_READWRITE),
            _ => (void*)Native.mmap(IntPtr.Zero, total, Native.PROT_READ | Native.PROT_WRITE,
                                    Native.MAP_PRIVATE | Native.MAP_ANONYMOUS, -1, 0),
        };

        var hdr = (AllocationHeader*)rawPtr;
        hdr->Magic = MagicValue;
        hdr->Size = size;

        var userPtr = (byte*)rawPtr + HeaderSize;
        _active[(nint)userPtr] = total;

        if (protection != MemoryProtectionMode.None)
            ApplyProtection(userPtr, size, protection);

        return userPtr;
    }

    public static void Free(void* userPtr,
                            NativeAllocatorBackend backend = NativeAllocatorBackend.PlatformInvoke)
    {
        if (userPtr is null) return;

        var key = (nint)userPtr;
        if (!_active.Remove(key, out var total))
            throw new InvalidOperationException("Double free or foreign pointer detected.");

        var hdr = (AllocationHeader*)((byte*)userPtr - HeaderSize);
        if (hdr->Magic != MagicValue)
            throw new InvalidOperationException("Foreign pointer detected.");
        hdr->Magic = FreedValue;

        var rawPtr = (void*)hdr;

        if (backend == NativeAllocatorBackend.DotNetUnmanaged)
        {
            NativeMemory.Free(rawPtr);
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            if (!Native.VirtualFree((nint)rawPtr, 0, Native.MEM_RELEASE))
                ThrowLastError("VirtualFree failed");
        }
        else if (Native.munmap((IntPtr)rawPtr, total) != 0)
        {
            ThrowLastError("munmap failed");
        }
    }

    public static void ApplyProtection(void* ptr, nuint size, MemoryProtectionMode mode)
    {
        if (ptr is null || size == 0) return;

        if (OperatingSystem.IsWindows())
        {
            var prot = mode switch
            {
                MemoryProtectionMode.ReadOnly => Native.PAGE_READONLY,
                MemoryProtectionMode.NoAccess => Native.PAGE_NOACCESS,
                _ => Native.PAGE_READWRITE,
            };
            if (!Native.VirtualProtect((nint)ptr, size, prot, out _))
                ThrowLastError("VirtualProtect failed");
        }
        else
        {
            var prot = mode switch
            {
                MemoryProtectionMode.ReadOnly => Native.PROT_READ,
                MemoryProtectionMode.NoAccess => Native.PROT_NONE,
                _ => Native.PROT_READ | Native.PROT_WRITE,
            };
            if (Native.mprotect((IntPtr)ptr, size, prot) != 0)
                ThrowLastError("mprotect failed");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowLastError(string msg)
        => throw new InvalidOperationException($"{msg} (errno {Marshal.GetLastWin32Error()})");
}

//------------------------------------------------------------------------------
//  platform interop (LibraryImport = compile-time P/Invoke stubs)
//------------------------------------------------------------------------------
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
