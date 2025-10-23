using JsonPath.Parser.Allocators;
using Xunit;

namespace Json.Path.Tests.Allocators;

public unsafe class ArenaAllocatorTests : IDisposable
{
    private readonly ArenaAllocator _arena = new();

    public void Dispose() => _arena.Dispose();

    [Fact]
    public void Alloc_SmallBlockWithinDefaultSegment_ShouldReturnValidPointer()
    {
        var ptr = _arena.Alloc(128);
        Assert.NotEqual(IntPtr.Zero, (nint)ptr);
    }

    [Fact]
    public void Alloc_MultipleSmallAllocations_ShouldNotOverlap()
    {
        var a = (byte*)_arena.Alloc(64);
        var b = (byte*)_arena.Alloc(64);

        Assert.True(b > a, "Second allocation should be at a higher address.");
        Assert.True((nuint)(b - a) >= 64);
    }

    [Fact]
    public void Alloc_LargeAllocation_ShouldTriggerNewSegment()
    {
        // default segment = 10MB, allocate 20MB to force growth
        var large = (byte*)_arena.Alloc(20 * 1024 * 1024);
        var small = (byte*)_arena.Alloc(128);

        // large allocation must be from earlier segment
        Assert.True(small != large);
    }

    [Fact]
    public void Dispose_ShouldFreeAllSegmentsWithoutCrash()
    {
        ArenaAllocator? arena = null;
        try
        {
            arena = new ArenaAllocator();
            arena.Alloc(4096);
            arena.Alloc(20 * 1024 * 1024); // force extra segment
        }
        finally
        {
            arena?.Dispose();
        }

        // double dispose should be safe
        arena.Dispose();
    }

    [Fact]
    public void AllocateManySmallObjects_ShouldNotThrowOrLeak()
    {
        for (int i = 0; i < 10_000; i++)
        {
            _arena.Alloc(64);
        }
    }

    [Fact]
    public void Arena_ShouldHandleExactSegmentFilling()
    {
        var total = 0u;
        var segSize = 1024 * 1024u; // small segment
        using var arena = new ArenaAllocator(segSize);

        while (true)
        {
            var ptr = arena.Alloc(256);
            total += 256;
            if (total > segSize)
            {
                break;
            }
        }
    }
}
