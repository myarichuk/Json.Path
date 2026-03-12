using JsonPath.Parser.Helpers;
using JsonPath.Parser.Lexer;

namespace JsonPath.Parser;

public readonly unsafe ref struct Parser(ArenaAllocator allocator)
{
    public AstHandle Parse(in ArenaList<Token> tokens)
    {
        var builder = new AstBuilder(allocator);

        var ctx = new ParserContext(tokens.AsPtr, tokens.Length);
        
        builder.AddSibling(AstKind.RootIdentifier); // root is always there
        var isAtEof = false;
        
        // Start parsing from the root
        if (ctx.Current.Kind == TokenKind.Root)
        {
            ctx.Consume();
        }

        while (!isAtEof)
        {
            var token = ctx.Current;
            switch (token.Kind)
            {
                case TokenKind.Eof:
                    isAtEof = true;
                    break;
                default:
                    ParseSegment(ref ctx);
                    break;
            }

            if (isAtEof)
            {
                break;
            }
        }
        
        return new AstHandle(builder.Root);
    }

    private void ParseSegment(ref ParserContext ctx)
    {
        // Placeholder for segment parsing logic.
        // For a skeleton parser, we simply consume the current token to advance the parser.
        ctx.Consume();
    }
}