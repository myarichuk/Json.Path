using System;
using Antlr4.Runtime;

namespace Json.Path.Tests.Infrastructure;

public class AntlrFixture<TLexer, TParser>
    where TLexer : Lexer
    where TParser : Parser
{
    public TLexer CreateLexer(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var charStream = new AntlrInputStream(input);
        var lexer = (TLexer?)Activator.CreateInstance(typeof(TLexer), charStream);

        return lexer ?? throw new InvalidOperationException($"Unable to create lexer of type {typeof(TLexer)}.");
    }

    public TParser CreateParser(string input)
    {
        var lexer = CreateLexer(input);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = (TParser?)Activator.CreateInstance(typeof(TParser), tokenStream);

        return parser ?? throw new InvalidOperationException($"Unable to create parser of type {typeof(TParser)}.");
    }
}
