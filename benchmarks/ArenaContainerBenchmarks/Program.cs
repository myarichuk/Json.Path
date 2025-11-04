using System.Buffers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using JsonPath.Parser.Helpers;

namespace ArenaBenchmarks;

/*
| Method                     | N    | Mean         | Ratio | RatioSD | Code Size | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |----- |-------------:|------:|--------:|----------:|-------:|-------:|----------:|------------:|
| List_Add_Iterate           | 10   |    131.14 ns |  1.01 |    0.17 |     193 B | 0.0451 |      - |     568 B |        1.00 |
| LinkedList_Add_Iterate     | 10   |    258.66 ns |  2.00 |    0.31 |     252 B | 0.0410 |      - |     520 B |        0.92 |
| ArrayPool_Add_Iterate      | 10   |     70.89 ns |  0.55 |    0.09 |   3,093 B |      - |      - |         - |        0.00 |
| ArenaList_Add_Iterate      | 10   |     30.42 ns |  0.24 |    0.03 |   1,165 B |      - |      - |         - |        0.00 |
| ArenaBlockList_Add_Iterate | 10   |     35.45 ns |  0.27 |    0.04 |   1,283 B |      - |      - |         - |        0.00 |
|                            |      |              |       |         |           |        |        |           |             |
| List_Add_Iterate           | 100  |    309.16 ns |  1.02 |    0.20 |     193 B | 0.0448 |      - |     568 B |        1.00 |
| LinkedList_Add_Iterate     | 100  |  1,458.99 ns |  4.81 |    0.83 |     252 B | 0.3853 | 0.0057 |    4840 B |        8.52 |
| ArrayPool_Add_Iterate      | 100  |    216.64 ns |  0.71 |    0.12 |   3,096 B |      - |      - |         - |        0.00 |
| ArenaList_Add_Iterate      | 100  |    199.44 ns |  0.66 |    0.11 |   1,165 B |      - |      - |         - |        0.00 |
| ArenaBlockList_Add_Iterate | 100  |    247.38 ns |  0.82 |    0.14 |   1,283 B |      - |      - |         - |        0.00 |
|                            |      |              |       |         |           |        |        |           |             |
| List_Add_Iterate           | 256  |    801.91 ns |  1.01 |    0.13 |     463 B | 0.1287 |      - |    1616 B |        1.00 |
| LinkedList_Add_Iterate     | 256  |  4,186.36 ns |  5.26 |    0.85 |     252 B | 0.9766 | 0.0381 |   12328 B |        7.63 |
| ArrayPool_Add_Iterate      | 256  |    561.90 ns |  0.71 |    0.10 |   5,377 B |      - |      - |         - |        0.00 |
| ArenaList_Add_Iterate      | 256  |    388.28 ns |  0.49 |    0.06 |   1,617 B |      - |      - |         - |        0.00 |
| ArenaBlockList_Add_Iterate | 256  |    594.55 ns |  0.75 |    0.14 |   1,252 B |      - |      - |         - |        0.00 |
|                            |      |              |       |         |           |        |        |           |             |
| List_Add_Iterate           | 512  |  1,597.81 ns |  1.01 |    0.14 |     463 B | 0.2937 | 0.0019 |    3688 B |        1.00 |
| LinkedList_Add_Iterate     | 512  |  9,425.14 ns |  5.96 |    1.85 |     254 B | 1.9531 | 0.1526 |   24616 B |        6.67 |
| ArrayPool_Add_Iterate      | 512  |  1,203.21 ns |  0.76 |    0.20 |   5,631 B |      - |      - |         - |        0.00 |
| ArenaList_Add_Iterate      | 512  |  1,037.94 ns |  0.66 |    0.08 |   1,601 B |      - |      - |         - |        0.00 |
| ArenaBlockList_Add_Iterate | 512  |  1,985.32 ns |  1.25 |    0.23 |   1,252 B |      - |      - |         - |        0.00 |
|                            |      |              |       |         |           |        |        |           |             |
| List_Add_Iterate           | 1024 |  5,052.37 ns |  1.01 |    0.17 |     463 B | 0.6180 |      - |    7808 B |        1.00 |
| LinkedList_Add_Iterate     | 1024 | 28,635.08 ns |  5.75 |    1.04 |     252 B | 3.9063 | 0.5493 |   49192 B |        6.30 |
| ArrayPool_Add_Iterate      | 1024 |  3,069.14 ns |  0.62 |    0.09 |   5,355 B |      - |      - |         - |        0.00 |
| ArenaList_Add_Iterate      | 1024 |  2,362.16 ns |  0.47 |    0.09 |   1,603 B |      - |      - |         - |        0.00 |
| ArenaBlockList_Add_Iterate | 1024 |  3,577.69 ns |  0.72 |    0.11 |   1,252 B |      - |      - |         - |        0.00 |
|                            |      |              |       |         |           |        |        |           |             |
| List_Add_Iterate           | 2048 | 10,148.73 ns |  1.01 |    0.14 |     463 B | 1.2665 | 0.0305 |   16024 B |        1.00 |
| LinkedList_Add_Iterate     | 2048 | 52,295.05 ns |  5.20 |    1.70 |     252 B | 7.8125 | 2.1973 |   98344 B |        6.14 |
| ArrayPool_Add_Iterate      | 2048 |  5,750.81 ns |  0.57 |    0.07 |   5,437 B |      - |      - |         - |        0.00 |
| ArenaList_Add_Iterate      | 2048 |  4,050.89 ns |  0.40 |    0.05 |   1,685 B |      - |      - |         - |        0.00 |
| ArenaBlockList_Add_Iterate | 2048 |  6,810.02 ns |  0.68 |    0.11 |   1,252 B |      - |      - |         - |        0.00 |
 */

[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 2)]
[HideColumns("Error", "StdDev", "Median")]
public unsafe class ArenaContainersBenchmarks
{
    [Params(10, 100, 256, 512, 1024, 2048)]
    public int N;

    private ArenaAllocator? _arena;

    [GlobalSetup]
    public void Setup()
    {
        _arena = new ArenaAllocator();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _arena?.Dispose();
    }

    // -----------------------------------------------------------
    // Standard .NET containers
    // -----------------------------------------------------------
    [Benchmark(Baseline = true)]
    public long List_Add_Iterate()
    {
        var list = new List<int>(128);
        for (int i = 0; i < N; i++)
        {
            list.Add(i);
        }

        long sum = 0;
        for (int i = 0; i < list.Count; i++)
        {
            sum += list[i];
        }

        return sum;
    }

    [Benchmark]
    public long LinkedList_Add_Iterate()
    {
        var list = new LinkedList<int>();
        for (int i = 0; i < N; i++)
        {
            list.AddLast(i);
        }

        long sum = 0;
        foreach (var v in list)
        {
            sum += v;
        }

        return sum;
    }

    [Benchmark]
    public long ArrayPool_Add_Iterate()
    {
        var pool = ArrayPool<int>.Shared;
        var capacity = 128;
        var arr = pool.Rent(capacity);
        int count = 0;

        try
        {
            for (int i = 0; i < N; i++)
            {
                if (count >= capacity)
                {
                    var newCapacity = capacity * 2;
                    var newArr = pool.Rent(newCapacity);
                    Array.Copy(arr, 0, newArr, 0, count);
                    pool.Return(arr, clearArray: false);
                    arr = newArr;
                    capacity = newCapacity;
                }

                arr[count++] = i;
            }

            long sum = 0;
            for (int i = 0; i < count; i++)
            {
                sum += arr[i];
            }

            return sum;
        }
        finally
        {
            pool.Return(arr, clearArray: false);
        }
    }

    // -----------------------------------------------------------
    // Arena-backed containers
    // -----------------------------------------------------------
    [Benchmark]
    public long ArenaList_Add_Iterate()
    {
        var list = new ArenaList<int>(_arena, 128);
        for (int i = 0; i < N; i++)
        {
            list.Add(i);
        }

        var span = list.AsSpan();
        long sum = 0;
        for (int i = 0; i < span.Length; i++)
        {
            sum += span[i];
        }

        _arena.Reset(); // reclaim memory
        return sum;
    }

    [Benchmark]
    public long ArenaBlockList_Add_Iterate()
    {
        var list = new ArenaBlockList<int>(_arena);
        for (int i = 0; i < N; i++)
        {
            list.Add(i);
        }

        long sum = 0;
        foreach (var v in list)
        {
            sum += v;
        }

        _arena.Reset();
        return sum;
    }
}

public static class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<ArenaContainersBenchmarks>();
    }
}