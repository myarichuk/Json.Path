using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;

namespace Json.Path.Parsing;

public class CollectingErrorListener : BaseErrorListener
{
    private readonly List<RecognitionException> _errors = new();
    public IList<RecognitionException> Errors => _errors;

    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
        string msg, RecognitionException e)
    {
        _errors.Add(e);
    }
}