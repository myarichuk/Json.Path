using JsonPath.Parser.Helpers;
using Xunit;

namespace Json.Path.Tests.Helpers;

public unsafe class ArenaStackTests
{
    [Fact]
    public void PushAndPop_ShouldReturnInLIFOOrder()
    {
        using var arena = new ArenaAllocator(4096);
        var stack = new ArenaStack<int>(arena);

        var a = (int*)arena.Alloc(sizeof(int));
        *a = 10;
        var b = (int*)arena.Alloc(sizeof(int));
        *b = 20;
        var c = (int*)arena.Alloc(sizeof(int));
        *c = 30;

        stack.Push(a);
        stack.Push(b);
        stack.Push(c);

        Assert.Equal(3, stack.Count);
        Assert.False(stack.IsEmpty);

        Assert.Equal(30, *stack.Pop());
        Assert.Equal(20, *stack.Pop());
        Assert.Equal(10, *stack.Pop());

        Assert.True(stack.IsEmpty);
    }

    [Fact]
    public void Peek_ShouldReturnTopWithoutRemoving()
    {
        using var arena = new ArenaAllocator(4096);
        var stack = new ArenaStack<int>(arena);

        var x = (int*)arena.Alloc(sizeof(int));
        *x = 1;
        var y = (int*)arena.Alloc(sizeof(int));
        *y = 2;

        stack.Push(x);
        stack.Push(y);

        var top = stack.Peek();
        Assert.Equal(2, *top);
        Assert.Equal(2, stack.Count);

        // Modify via pointer
        *top = 99;
        Assert.Equal(99, *stack.Peek());
    }

    [Fact]
    public void Clear_ShouldResetCount()
    {
        using var arena = new ArenaAllocator(4096);
        var stack = new ArenaStack<int>(arena);

        for (int i = 0; i < 3; i++)
        {
            var ptr = (int*)arena.Alloc(sizeof(int));
            *ptr = i;
            stack.Push(ptr);
        }

        stack.Clear();

        Assert.True(stack.IsEmpty);
        Assert.Equal(0, stack.Count);

        var newPtr = (int*)arena.Alloc(sizeof(int));
        *newPtr = 42;
        stack.Push(newPtr);

        Assert.Equal(1, stack.Count);
        Assert.Equal(42, *stack.Peek());
    }

    [Fact]
    public void Pop_OnEmpty_ShouldThrow()
    {
        using var arena = new ArenaAllocator(4096);
        var stack = new ArenaStack<int>(arena);

        Assert.Throws<InvalidOperationException>(() => stack.Pop());
    }

    [Fact]
    public void Peek_OnEmpty_ShouldThrow()
    {
        using var arena = new ArenaAllocator(4096);
        var stack = new ArenaStack<int>(arena);

        Assert.Throws<InvalidOperationException>(() => stack.Peek());
    }

    [Fact]
    public void PushBeyondInitialCapacity_ShouldGrow()
    {
        using var arena = new ArenaAllocator(4096);
        var stack = new ArenaStack<int>(arena, initialCapacity: 2);

        for (int i = 0; i < 10; i++)
        {
            var ptr = (int*)arena.Alloc(sizeof(int));
            *ptr = i;
            stack.Push(ptr);
        }

        Assert.Equal(10, stack.Count);

        for (int i = 9; i >= 0; i--)
        {
            Assert.Equal(i, *stack.Pop());
        }

        Assert.True(stack.IsEmpty);
    }

    [Fact]
    public void CanHandleUnmanagedStruct()
    {
        using var arena = new ArenaAllocator(4096);
        var stack = new ArenaStack<Foobar>(arena, initialCapacity: 4);

        var f1 = (Foobar*)arena.Alloc((uint)sizeof(Foobar));
        *f1 = new Foobar { X = 1, Y = 2 };

        var f2 = (Foobar*)arena.Alloc((uint)sizeof(Foobar));
        *f2 = new Foobar { X = 3, Y = 4 };

        stack.Push(f1);
        stack.Push(f2);

        var popped = stack.Pop();
        Assert.Equal(3, popped->X);
        Assert.Equal(4, popped->Y);
    }

    private struct Foobar
    {
        public int X;
        public int Y;
    }
}
