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

        expr.BeginExpression(AstKind.ChildSegment, 123)
                .BeginExpression(AstKind.NameSelector, "AAA")
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

        expr.ChildExpression(AstKind.ChildSegment, 0)
                .SiblingExpression(AstKind.NameSelector, "AAA")
                .SiblingExpression(AstKind.WildcardSelector, 0);

        var first = builder.Root->NextChild;
        Assert.Equal(AstKind.ChildSegment, first->Kind);

        var second = first->NextSibling;
        Assert.Equal(AstKind.NameSelector, second->Kind);

        var third = second->NextSibling;
        Assert.Equal(AstKind.WildcardSelector, third->Kind);

        Assert.True(third->NextSibling == null);

        // Ensure the root scope did not change while adding siblings.
        Assert.True(builder.Root->NextSibling == null);

        // The helper should allow adding siblings relative to a known node pointer.
        var fourth = builder.AddSiblingAfter(third, AstKind.FilterExpression);
        Assert.True(third->NextSibling == fourth.Ptr);
        Assert.Equal(AstKind.FilterExpression, fourth.Ptr->Kind);
        Assert.True(fourth.Ptr->NextSibling == null);
    }

}
