using Antlr4.Runtime;

namespace Json.Path.Parsing;

internal class TolerantErrorStrategy : DefaultErrorStrategy
{
    public override void Recover(Parser recognizer, RecognitionException e)
    {
        // skip bad tokens
        recognizer.Consume();
    }
}
