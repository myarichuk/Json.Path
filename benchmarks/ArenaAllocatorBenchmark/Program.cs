using System;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using JsonPath.Parser.Allocators;
using Varena;

BenchmarkRunner.Run<ArenaBenchmark>();

[MemoryDiagnoser]
public class ArenaBenchmark
{
    // test allocation sizes
    [Params(64, 1024, 16 * 1024, 256 * 1024)]
    public int Size;

    // number of allocations per "request"
    [Params(10, 100, 1000)]
    public int AllocationsPerRequest;

    private const nuint ArenaInitialSize = 10 * 1024 * 1024; // 10MB
    private const ulong VarenaBufferSize = 128 * 1024 * 1024; // 128MB virtual space

    [Benchmark(Baseline = true, Description = "NativeMemory.Alloc/Free")]
    public unsafe void Baseline_UnmanagedAlloc()
    {
        for (int i = 0; i < AllocationsPerRequest; i++)
        {
            void* ptr = NativeMemory.Alloc((nuint)Size);
            NativeMemory.Free(ptr);
        }
    }

    [Benchmark(Description = "ArenaAllocator [P/Invoke] (create -> alloc -> dispose)")]
    public unsafe void ArenaAllocator_Session()
    {
        using var arena = new ArenaAllocator(ArenaInitialSize);
        for (int i = 0; i < AllocationsPerRequest; i++)
        {
            _ = arena.Alloc((nuint)Size);
        }
    }

    [Benchmark(Description = "ArenaAllocator [NativeMemory.Alloc] (create -> alloc -> dispose)")]
    public unsafe void ArenaAllocator_Native_Session()
    {
        using var arena = new ArenaAllocator(ArenaInitialSize);
        for (int i = 0; i < AllocationsPerRequest; i++)
        {
            _ = arena.Alloc((nuint)Size);
        }
    }

    [Benchmark(Description = "Varena (create -> alloc -> dispose)")]
    public void Varena_Session()
    {
        using var mgr = new VirtualArenaManager();
        using var buffer = mgr.CreateBuffer("BenchArena", (UIntPtr)VarenaBufferSize);

        for (int i = 0; i < AllocationsPerRequest; i++)
        {
            var span = buffer.AllocateRange(Size);
            span[0] = 123; // ensure page commit
        }
    }
}