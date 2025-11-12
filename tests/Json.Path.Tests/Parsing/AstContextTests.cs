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

        var node = AllocNode();
        var name = "foo";

        writer.AssignNode(node, ref data, AstKind.NameSelector, name);

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

        var literal = new LiteralData { Kind = LiteralKind.Number, Number = 42 };

        var node = AllocNode();
        writer.AssignNode(node, ref data, AstKind.LiteralValue, literal);

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

        var node = AllocNode();
        writer.AssignNode(node, ref data, AstKind.IndexSelector, index);

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

        var node = AllocNode();
        writer.AssignNode(node, ref data, AstKind.SliceSelector, slice);

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

        var node = AllocNode();
        writer.AssignNode(node, ref data, AstKind.FunctionCall, fn);

        data.AddFunctionArgument(node->DataIndex, new FunctionArgument
        {
            ArgName = ArenaString.Clone("foo", _allocator),
            Kind = TokenKind.False,
            Value = null,
        });

        var readBack = reader.GetData<FunctionCallData>(ref data, node);

        Assert.Equal("length", readBack.Name.ToString());
        Assert.Equal((ushort)1, readBack.ArgumentCount);

        var args = data.GetFunctionArguments(node->DataIndex);
        Assert.Equal(1, args.Length);
        Assert.Equal("foo", args[0].ArgName.ToString());
        Assert.Equal(TokenKind.False, args[0].Kind);
    }


    [Fact]
    public void RootIdentifier_NoData()
    {
        var data = new AstDataTable(_allocator);
        var writer = new AstWriteContext(_allocator);

        var node = AllocNode();

        writer.AssignNode(node, ref data, AstKind.RootIdentifier, 0);

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

        var node = AllocNode();

        try
        {
            writer.AssignNode(node, ref data, AstKind.IndexSelector, "wrong-type");
        }
        catch (ArgumentException)
        {
            return;
        }
        
        Assert.Fail("Must throw ArgumentException..");
    }
    
    private AstNode* AllocNode()
    {
        var node = (AstNode*)_allocator.Alloc((nuint)sizeof(AstNode));
        node->Kind = 0;
        node->NextChild = null;
        node->NextSibling = null;
        node->DataIndex = 0;
        return node;
    }
}