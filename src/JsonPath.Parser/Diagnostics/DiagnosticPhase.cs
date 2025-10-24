namespace JsonPath.Parser.Diagnostics;

public enum DiagnosticPhase : byte
{
    Unknown = 0,
    Lexer,
    Parser,
    Semantic
}