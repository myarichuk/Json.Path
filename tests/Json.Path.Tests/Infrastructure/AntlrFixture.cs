using System.Diagnostics.CodeAnalysis;
using Antlr4.Runtime;
using Json.Path.Parsing;

// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable ClassNeverInstantiated.Global
namespace Json.Path.Tests.Infrastructure;

[SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Testing code, don't care")]
public class AntlrFixture<TLexer, TParser>
    where TLexer : Lexer
    where TParser : Parser
{
    internal TParser CreateParser(string input)
    {
        var lexer = CreateLexer(input);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = (TParser?)Activator.CreateInstance(typeof(TParser), tokenStream);

        Validator = new SemanticErrorListener(tokenStream);
        parser?.AddParseListener(Validator);

        parser?.ErrorHandler = _errorStrategy;
        return parser ?? throw new InvalidOperationException($"Unable to create parser of type {typeof(TParser)}.");
    }

    private TLexer CreateLexer(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var charStream = new AntlrInputStream(input);
        var lexer = (TLexer?)Activator.CreateInstance(typeof(TLexer), charStream);

        return lexer ?? throw new InvalidOperationException($"Unable to create lexer of type {typeof(TLexer)}.");
    }

    private readonly TolerantErrorStrategy _errorStrategy = new();

    public IReadOnlyList<RecognitionException> SyntaxErrors => _errorStrategy.Errors;

    public IReadOnlyList<SemanticError> SemanticErrors => Validator?.Errors ?? [];

    public SemanticErrorListener? Validator { get; private set; }
}
