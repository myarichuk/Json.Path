using JsonPath.Parser;
using Xunit;

namespace Json.Path.Tests.Parsing;

public unsafe class AstBuilderTests
{
    [Fact]
    public void BeginAndEndCreatesNestedStructure()
    {
        using var arena = new ArenaAllocator();
        var builder = new AstBuilder(arena);
        var data = new AstDataTable(arena);
        var expr = new AstExpressionBuilder(arena, builder, data, new AstHandle(builder.Root));

        expr.BeginExpression(AstKind.ChildSegment)
                .BeginNameSelector("AAA")
                .EndExpression()
            .EndExpression();

        var root = builder.Root;
        Assert.Equal(AstKind.RootIdentifier, root->Kind);

        var child = root->NextChild;
        Assert.True(child != null);
        Assert.Equal(AstKind.ChildSegment, child->Kind);

        var nested = child->NextChild;
        Assert.True(nested != null);
        Assert.Equal(AstKind.NameSelector, nested->Kind);
    }

    [Fact]
    public void AddSiblingCreatesCorrectSiblingChain()
    {
        using var arena = new ArenaAllocator();
        var builder = new AstBuilder(arena);
        var data = new AstDataTable(arena);
        var expr = new AstExpressionBuilder(arena, builder, data, new AstHandle(builder.Root));

        expr.ChildExpression(AstKind.ChildSegment)
                .SiblingNameSelector("AAA")
                .SiblingExpression(AstKind.WildcardSelector);

        var first = builder.Root->NextChild;
        Assert.Equal(AstKind.ChildSegment, first->Kind);

        var second = first->NextSibling;
        Assert.Equal(AstKind.NameSelector, second->Kind);

        var third = second->NextSibling;
        Assert.Equal(AstKind.WildcardSelector, third->Kind);

        Assert.True(third->NextSibling == null);
    }

    [Fact]
    public void EndExpressionTracksLastHandle()
    {
        using var arena = new ArenaAllocator();
        var builder = new AstBuilder(arena);
        var data = new AstDataTable(arena);
        var expr = new AstExpressionBuilder(arena, builder, data, new AstHandle(builder.Root));

        expr.BeginExpression(AstKind.ChildSegment)
                .ChildNameSelector("foo")
            .EndExpression()
            .SiblingExpression(AstKind.ChildSegment);

        var first = builder.Root->NextChild;
        Assert.Equal(AstKind.ChildSegment, first->Kind);

        var sibling = first->NextSibling;
        Assert.True(sibling != null);
        Assert.Equal(AstKind.ChildSegment, sibling->Kind);
    }
}
