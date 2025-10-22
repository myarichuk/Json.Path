using System;
using System.Globalization;
using Antlr4.Runtime;
using Json.Path.Parsing;

// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable ClassNeverInstantiated.Global
namespace Json.Path.Tests.Infrastructure;

public class AntlrFixture<TLexer, TParser>
    where TLexer : Lexer
    where TParser : Parser
{
    internal TParser CreateParser(string input)
    {
        var lexer = CreateLexer(input);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = (TParser?)Activator.CreateInstance(typeof(TParser), tokenStream);

        Validator = new JsonPathSemanticValidator(tokenStream);
        parser?.AddParseListener(Validator);
        parser?.ErrorHandler = new TolerantErrorStrategy();
        return parser ?? throw new InvalidOperationException($"Unable to create parser of type {typeof(TParser)}.");
    }

    private TLexer CreateLexer(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var charStream = new AntlrInputStream(input);
        var lexer = (TLexer?)Activator.CreateInstance(typeof(TLexer), charStream);

        return lexer ?? throw new InvalidOperationException($"Unable to create lexer of type {typeof(TLexer)}.");
    }

    public JsonPathSemanticValidator? Validator { get; private set; }
}
