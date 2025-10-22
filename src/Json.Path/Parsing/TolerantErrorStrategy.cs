using System.Collections.Generic;
using Antlr4.Runtime;

namespace Json.Path.Parsing;

internal class TolerantErrorStrategy : DefaultErrorStrategy
{
    private readonly List<RecognitionException> _errors = new();

    public IReadOnlyList<RecognitionException> Errors => _errors;


    public override void Recover(Parser recognizer, RecognitionException e)
    {
        _errors.Add(e);
        
        // skip bad tokens
        recognizer.Consume();
    }
}
