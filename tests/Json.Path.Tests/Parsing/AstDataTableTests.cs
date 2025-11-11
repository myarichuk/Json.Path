using JsonPath.Parser;
using JsonPath.Parser.Helpers;
using JsonPath.Parser.Lexer;
using Xunit;

namespace Json.Path.Tests.Parsing;

public class AstDataTableTests : IDisposable
{
    private readonly ArenaAllocator _arena = new();

    public void Dispose() => _arena.Dispose();

    [Fact]
    public void AddAndRetrieve_NameSelector_Works()
    {
        var data = new AstDataTable(_arena);

        var str = ArenaString.Clone("foo", _arena);
        var idx = data.AddName(str);

        var retrieved = data.GetName(idx);
        Assert.False(retrieved.IsEmpty);
        Assert.Equal("foo", retrieved.ToString());
    }

    [Fact]
    public void AddAndRetrieve_IndexSelector_Works()
    {
        var data = new AstDataTable(_arena);

        var idx = data.AddIndex(42);
        var value = data.GetIndex(idx);

        Assert.Equal(42, value);
    }

    [Fact]
    public void AddAndRetrieve_Slice_Works()
    {
        var data = new AstDataTable(_arena);

        var slice = new SliceData
        {
            Start = 1,
            End = 10,
            Step = 2,
            Flags = SliceFlags.HasStart | SliceFlags.HasEnd | SliceFlags.HasStep
        };

        var idx = data.AddSlice(slice);
        var retrieved = data.GetSlice(idx);

        Assert.True(retrieved.HasStart);
        Assert.True(retrieved.HasEnd);
        Assert.True(retrieved.HasStep);

        Assert.Equal(1, retrieved.Start);
        Assert.Equal(10, retrieved.End);
        Assert.Equal(2, retrieved.Step);
    }

    [Fact]
    public unsafe void AddAndRetrieve_FunctionCall_Works()
    {
        var data = new AstDataTable(_arena);

        var fn = FunctionCallData.From("length", _arena);
        fn.Args->Add(new FunctionArgument
        {
            ArgName = ArenaString.Clone("foo", _arena),
            Kind = TokenKind.False,
            Value = null,
        });

        var idx = data.AddFunction(fn);
        var retrieved = data.GetFunction(idx);

        Assert.Equal("length", retrieved.Name.ToString());
        Assert.Equal((ushort)1, retrieved.Args->Length);
    }

    [Fact]
    public void AddAndRetrieve_LiteralValues_Works()
    {
        var data = new AstDataTable(_arena);

        var litStr = new LiteralData
        {
            Kind = LiteralKind.String,
            String = ArenaString.Clone("bar", _arena)
        };
        var litNum = new LiteralData
        {
            Kind = LiteralKind.Number,
            Number = 123.45
        };
        var litBool = new LiteralData
        {
            Kind = LiteralKind.Boolean,
            Bool = 1
        };
        var litNull = new LiteralData { Kind = LiteralKind.Null };

        var strIdx = data.AddLiteral(litStr);
        var numIdx = data.AddLiteral(litNum);
        var boolIdx = data.AddLiteral(litBool);
        var nullIdx = data.AddLiteral(litNull);

        Assert.Equal("\"bar\"", data.GetLiteral(strIdx).ToString());
        Assert.Equal("123.45", data.GetLiteral(numIdx).ToString());
        Assert.Equal("true", data.GetLiteral(boolIdx).ToString());
        Assert.Equal("null", data.GetLiteral(nullIdx).ToString());
    }

    [Fact]
    public void AddMultiple_Names_Work()
        {
            var data = new AstDataTable(_arena);

            var n1 = data.AddName(ArenaString.Clone("alpha", _arena));
            var n2 = data.AddName(ArenaString.Clone("beta", _arena));
            var n3 = data.AddName(ArenaString.Clone("gamma", _arena));

            Assert.Equal(0u, n1);
            Assert.Equal(1u, n2);
            Assert.Equal(2u, n3);

            Assert.Equal("alpha", data.GetName(n1).ToString());
            Assert.Equal("beta", data.GetName(n2).ToString());
            Assert.Equal("gamma", data.GetName(n3).ToString());
        }

    [Fact]
    public void AddMultiple_Indices_Work()
        {
            var data = new AstDataTable(_arena);

            var i1 = data.AddIndex(10);
            var i2 = data.AddIndex(20);
            var i3 = data.AddIndex(30);

            Assert.Equal(0u, i1);
            Assert.Equal(1u, i2);
            Assert.Equal(2u, i3);

            Assert.Equal(10, data.GetIndex(i1));
            Assert.Equal(20, data.GetIndex(i2));
            Assert.Equal(30, data.GetIndex(i3));
        }

    [Fact]
    public void AddMultiple_Literals_Work()
        {
            var data = new AstDataTable(_arena);

            var lit1 = new LiteralData { Kind = LiteralKind.Number, Number = 1 };
            var lit2 = new LiteralData { Kind = LiteralKind.String, String = ArenaString.Clone("foo", _arena) };
            var lit3 = new LiteralData { Kind = LiteralKind.Boolean, Bool = 1 };

            var idx1 = data.AddLiteral(lit1);
            var idx2 = data.AddLiteral(lit2);
            var idx3 = data.AddLiteral(lit3);

            Assert.Equal(0u, idx1);
            Assert.Equal(1u, idx2);
            Assert.Equal(2u, idx3);

            Assert.Equal("1", data.GetLiteral(idx1).ToString());
            Assert.Equal("\"foo\"", data.GetLiteral(idx2).ToString());
            Assert.Equal("true", data.GetLiteral(idx3).ToString());
        }

    [Fact]
    public void AddMultiple_Slices_Work()
        {
            var data = new AstDataTable(_arena);

            var s1 = new SliceData { Start = 0, End = 5, Step = 1, Flags = SliceFlags.HasStart | SliceFlags.HasEnd | SliceFlags.HasStep };
            var s2 = new SliceData { Start = 2, End = 8, Step = 2, Flags = SliceFlags.HasStart | SliceFlags.HasEnd | SliceFlags.HasStep };

            var idx1 = data.AddSlice(s1);
            var idx2 = data.AddSlice(s2);

            Assert.Equal(0u, idx1);
            Assert.Equal(1u, idx2);

            var r1 = data.GetSlice(idx1);
            var r2 = data.GetSlice(idx2);

            Assert.Equal("[0:5:1]", r1.ToString());
            Assert.Equal("[2:8:2]", r2.ToString());
        }

    [Fact]
    public unsafe void AddMultiple_Functions_Work()
        {
            var data = new AstDataTable(_arena);

            var fnA = FunctionCallData.From("length", _arena);
            var fnB = FunctionCallData.From("sum", _arena);

            fnA.Args->Add(new FunctionArgument
            {
                ArgName = ArenaString.Clone("count", _arena),
            });

            fnB.Args->Add(new FunctionArgument
            {
                ArgName = ArenaString.Clone("a", _arena),
            });
            fnB.Args->Add(new FunctionArgument
            {
                ArgName = ArenaString.Clone("b", _arena),
            });

            var idxA = data.AddFunction(fnA);
            var idxB = data.AddFunction(fnB);

            Assert.Equal(0u, idxA);
            Assert.Equal(1u, idxB);

            var f1 = data.GetFunction(idxA);
            var f2 = data.GetFunction(idxB);

            Assert.Equal("length", f1.Name.ToString());
            Assert.Equal("sum", f2.Name.ToString());

            Assert.Equal((ushort)1, f1.Args->Length);
            Assert.Equal((ushort)2, f2.Args->Length);
        }

    [Fact]
    public unsafe void AddingDifferentTypes_DoesNotInterfere()
        {
            var data = new AstDataTable(_arena);

            var nameIdx = data.AddName(ArenaString.Clone("prop", _arena));
            var litIdx = data.AddLiteral(new LiteralData { Kind = LiteralKind.Number, Number = 99 });
            var fn = FunctionCallData.From("exists", _arena);
            var fnIdx = data.AddFunction(fn);

            fn.Args->Add(new FunctionArgument
            {
                ArgName = ArenaString.Clone("item", _arena),
                Kind = TokenKind.Number,
            });

            Assert.Equal("prop", data.GetName(nameIdx).ToString());
            Assert.Equal("99", data.GetLiteral(litIdx).ToString());
            Assert.Equal("exists", data.GetFunction(fnIdx).Name.ToString());
        }

    [Fact]
    public void LargeNumberOfEntries_Work()
        {
            var data = new AstDataTable(_arena);

            const int count = 500;
            for (int i = 0; i < count; i++)
            {
                var name = ArenaString.Clone($"item{i}", _arena);
                data.AddName(name);
            }

            for (int i = 0; i < count; i += 50)
            {
                var name = data.GetName((uint)i).ToString();
                Assert.Equal($"item{i}", name);
            }
        }
}