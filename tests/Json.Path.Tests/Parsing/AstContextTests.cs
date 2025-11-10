using System;
using Xunit;
using JsonPath.Parser;
using JsonPath.Parser.Helpers;
using JsonPath.Parser.Lexer;

namespace Json.Path.Tests;

public unsafe class AstContextTests : IDisposable
{
    private readonly ArenaAllocator _allocator = new(4096);

    public void Dispose() => _allocator.Dispose();

    [Fact]
    public void NameSelector_RoundTrip()
    {
        var data = new AstDataTable(_allocator);
        var writer = new AstWriteContext(_allocator);
        var reader = default(AstReadContext);

        var name = "foo";
        var node = writer.CreateNode(ref data, AstKind.NameSelector, name);
        Assert.True(node != null);
        Assert.Equal(AstKind.NameSelector, node->Kind);

        var arenaName = reader.GetData<ArenaString>(ref data, node);
        Assert.Equal(name, arenaName.ToString());
    }

    [Fact]
    public void LiteralValue_RoundTrip()
    {
        var data = new AstDataTable(_allocator);
        var writer = new AstWriteContext(_allocator);
        var reader = default(AstReadContext);

        var literal = new LiteralData
        {
            Kind = LiteralKind.Number,
            Number = 42,
        };
        var node = writer.CreateNode(ref data, AstKind.LiteralValue, literal);

        var result = reader.GetData<LiteralData>(ref data, node);
        Assert.Equal(literal, result);
    }

    [Fact]
    public void IndexSelector_RoundTrip()
    {
        var data = new AstDataTable(_allocator);
        var writer = new AstWriteContext(_allocator);
        var reader = default(AstReadContext);

        const long index = 5;
        var node = writer.CreateNode(ref data, AstKind.IndexSelector, index);
        var readBack = reader.GetData<long>(ref data, node);

        Assert.Equal(index, readBack);
    }

    [Fact]
    public void SliceSelector_RoundTrip()
    {
        var data = new AstDataTable(_allocator);
        var writer = new AstWriteContext(_allocator);
        var reader = default(AstReadContext);

        var slice = new SliceData
        {
            Start = 1,
            End = 10,
            Step = 2,
            Flags = SliceFlags.HasStart | SliceFlags.HasEnd | SliceFlags.HasStep,
        };
        var node = writer.CreateNode(ref data, AstKind.SliceSelector, slice);
        var readBack = reader.GetData<SliceData>(ref data, node);

        Assert.Equal(slice, readBack);
    }

    [Fact]
    public void FunctionCall_RoundTrip()
    {
        var data = new AstDataTable(_allocator);
        var writer = new AstWriteContext(_allocator);
        var reader = default(AstReadContext);

        var fn = FunctionCallData.From("length", _allocator);
        fn.Args->Add(new FunctionArgument
        {
            ArgName = ArenaString.Clone("foo", _allocator),
            Kind = TokenKind.False,
            Value = null,
        });

        var node = writer.CreateNode(ref data, AstKind.FunctionCall, fn);
        var readBack = reader.GetData<FunctionCallData>(ref data, node);

        Assert.Equal(fn, readBack);
    }

    [Fact]
    public void RootIdentifier_NoData()
    {
        var data = new AstDataTable(_allocator);
        var writer = new AstWriteContext(_allocator);

        var node = writer.CreateNode(ref data, AstKind.RootIdentifier, 0);
        Assert.Equal(0u, node->DataIndex);
    }

    [Fact]
    public void NullNode_ShouldReturnDefaults()
    {
        var data = new AstDataTable(_allocator);
        var reader = default(AstReadContext);

        AstNode* nullNode = null;
        Assert.Null(reader.GetData(ref data, nullNode));
        Assert.Equal(0, reader.GetData<long>(ref data, nullNode));
    }

    [Fact]
    public void InvalidType_ShouldThrow_OnWrite()
    {
        var data = new AstDataTable(_allocator);
        var writer = new AstWriteContext(_allocator);

        try
        {
            writer.CreateNode(ref data, AstKind.IndexSelector, "wrong-type");
        }
        catch (ArgumentException)
        {
            return;
        }

        Assert.Fail("Should have thrown " + nameof(ArgumentException));
    }

    [Fact]
    public void InvalidType_ShouldThrow_OnRead()
    {
        var data = new AstDataTable(_allocator);
        var writer = new AstWriteContext(_allocator);
        var reader = default(AstReadContext);

        var node = writer.CreateNode(ref data, AstKind.LiteralValue, new LiteralData
        {
            Kind = LiteralKind.Number,
            Number = 123,
        });

        try
        {
            _ = reader.GetData<long>(ref data, node);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        Assert.Fail("Should have thrown " + nameof(InvalidOperationException));
    }

    [Fact]
    public void NodeInitialization_ShouldBeValid()
    {
        var data = new AstDataTable(_allocator);
        var writer = new AstWriteContext(_allocator);

        // force > 0 data index
        _ = writer.CreateNode(
            ref data,
            AstKind.LiteralValue,
            new LiteralData
            {
                Kind = LiteralKind.Number,
                Number = 1,
            });

        var node = writer.CreateNode(
            ref data,
            AstKind.LiteralValue,
            new LiteralData
        {
            Kind = LiteralKind.Number,
            Number = 1,
        });

        Assert.True(node->NextChild == null);
        Assert.True(node->NextSibling == null);
        Assert.Equal(AstKind.LiteralValue, node->Kind);
        Assert.Equal(1, (int)node->DataIndex);
    }
}