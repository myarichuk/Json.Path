using JsonPath.Parser.Helpers;
using JsonPath.Parser.Lexer;

namespace JsonPath.Parser;

public readonly unsafe ref struct Parser(ArenaAllocator allocator)
{
    public AstHandle Parse(in ArenaList<Token> tokens)
    {
        var builder = new AstBuilder(allocator);
        var data = new AstDataTable(allocator);
        
        var expressionBuilder = 
            new AstExpressionBuilder(allocator, builder, data, builder.RootHandle);
        
        builder.AddSibling(AstKind.RootIdentifier); // root is always there
        var isAtEof = false;
        
        for (int i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            switch (token.Kind)
            {
                case TokenKind.Eof:
                    isAtEof = true;
                    break;
                case TokenKind.Identifier:
                    break;
                case TokenKind.Number:
                    break;
                case TokenKind.String:
                    break;
                case TokenKind.Root:
                    break;
                case TokenKind.Current:
                    break;
                case TokenKind.DotDot:
                    break;
                case TokenKind.Dot:
                    break;
                case TokenKind.LBracket:
                    break;
                case TokenKind.RBracket:
                    break;
                case TokenKind.LParen:
                    break;
                case TokenKind.RParen:
                    break;
                case TokenKind.Colon:
                    break;
                case TokenKind.Comma:
                    break;
                case TokenKind.Question:
                    break;
                case TokenKind.Star:
                    break;
                case TokenKind.RegexMatch:
                    break;
                case TokenKind.Eq:
                    break;
                case TokenKind.Ne:
                    break;
                case TokenKind.Le:
                    break;
                case TokenKind.Lt:
                    break;
                case TokenKind.Ge:
                    break;
                case TokenKind.Gt:
                    break;
                case TokenKind.And:
                    break;
                case TokenKind.Or:
                    break;
                case TokenKind.Not:
                    break;
                case TokenKind.Add:
                    break;
                case TokenKind.Sub:
                    break;
                case TokenKind.Div:
                    break;
                case TokenKind.Mod:
                    break;
                case TokenKind.True:
                    break;
                case TokenKind.False:
                    break;
                case TokenKind.Null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (isAtEof)
            {
                break;
            }
        }
        
        return new AstHandle(builder.Root);
    }
}