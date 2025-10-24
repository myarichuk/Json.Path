namespace JsonPath.Parser.Lexer;

public interface ISubScanner
{
    bool TryScan(ref ScanContext ctx, out Token token);
}