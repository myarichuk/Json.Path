#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable PossibleMultipleEnumeration
namespace Json.Path.Parsing;

public record struct SemanticError(
    string Code,
    string Message,
    Interval Span);

// A listener that inspects the parse tree and uses the token stream to check RFC constraints
public sealed class SemanticErrorListener(CommonTokenStream tokens) : JsonPathBaseListener
{
    private static readonly HashSet<string> KnownFunctions = new(StringComparer.InvariantCultureIgnoreCase) 
        { "length", "count", "match", "search", "value" };

    private readonly List<SemanticError> _errors = [];
    public IReadOnlyList<SemanticError> Errors => _errors;

    #region Helpers
    private void Add(string code, IToken tok, string message)
        => _errors.Add(new SemanticError(code, message, new Interval(tok.StartIndex, tok.StopIndex)));

    private bool HasHiddenWsToLeft(IToken? t)
    {
        if (t is { TokenIndex: < 0 })
        {
            return false;
        }

        var hidden = tokens.GetHiddenTokensToLeft(t!.TokenIndex);
        return hidden is { Count: > 0 };
    }

    private bool HasHiddenWsToRight(IToken? t)
    {
        if (t is { TokenIndex: < 0 })
        {
            return false;
        }

        var hidden = tokens.GetHiddenTokensToRight(t!.TokenIndex);
        return hidden is { Count: > 0 };
    }

    private bool HasHiddenWsAround(IToken? t)
        => HasHiddenWsToLeft(t) || HasHiddenWsToRight(t);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsZeroToken(IToken? t)
        => t?.Text is "0" or "+0" or "-0";
    #endregion
    
    public override void ExitSliceSelector(JsonPathParser.SliceSelectorContext ctx)
    {
        void CheckNoWs(IToken? t)
        {
            if (t != null && HasHiddenWsAround(t))
            {
                Add("SliceWhitespace", t, "Whitespace inside slice is not allowed");
            }
        }

        CheckNoWs(ctx.startIndex);
        CheckNoWs(ctx.c1);
        CheckNoWs(ctx.endIndex);
        CheckNoWs(ctx.c2);
        CheckNoWs(ctx.step);

        if (ctx.startIndex is { Text: var si } && IsFloatLike(si))
        {
            Add("SliceNonIntegerStart", ctx.startIndex, "Slice start index must be integer");
        }

        if (ctx.endIndex is { Text: var ei } && IsFloatLike(ei))
        {
            Add("SliceNonIntegerEnd", ctx.endIndex, "Slice end index must be integer");
        }

        if (ctx.step is { Text: var st } && IsFloatLike(st))
        {
            Add("SliceNonIntegerStep", ctx.step, "Slice step must be integer");
        }

        if (IsZeroToken(ctx.step))
        {
            Add("SliceZeroStep", ctx.step!, "Slice step must be non-zero");
        }

        CheckLeadingZero(ctx.startIndex);
        CheckLeadingZero(ctx.endIndex);
        CheckLeadingZero(ctx.step);
        return;

        void CheckLeadingZero(IToken? t)
        {
            if (t?.Text is { Length: > 1 } text
                && text[0] == '0'
                && char.IsDigit(text[1]))
            {
                Add("LeadingZero", t, "Leading zeros are not allowed in numbers");
            }
        }

        static bool IsFloatLike(string s) => s.Contains('.') || s.Contains('e') || s.Contains('E');
    }

    public override void ExitFunctionExpression(JsonPathParser.FunctionExpressionContext ctx)
    {
        var functionName = ctx.function.Text;

        if (!KnownFunctions.Contains(functionName))
        {
            Add("UnknownFunction", ctx.function, 
                $"Function '{functionName}' is not a registered JSONPath function");
            return;
        }

        var argCount = ctx._params?.Count ?? 0;
        switch (functionName)
        {
            case "length" or "count" or "value" when argCount != 1:
                    Add("FunctionsParamCount", ctx.function, $"Function '{functionName}' expects exactly one argument");
                break;
            case "match" or "search" when argCount != 2:
                    Add("FunctionsParamCount", ctx.function, $"Function '{functionName}' expects exactly two arguments");
                break;
            case "match" or "search" 
                when ctx._params?.ElementAtOrDefault(1) is not JsonPathParser.StringLiteralExpressionContext:
                    Add("FunctionArgType", ctx.function, $"Function '{functionName}' second argument must be a string literal");
                break;            
        }
        
        // whitespace validations
        var lp = ctx.LPAREN().Symbol;
        var rp = ctx.RPAREN().Symbol;
        if (HasHiddenWsToRight(lp))
        {
            Add("FuncWhitespace", lp, "Whitespace not allowed after function name");
        }

        if (HasHiddenWsToLeft(rp))
        {
            Add("FuncWhitespace", rp, "Whitespace not allowed before closing parenthesis");
        }
    }

    // descendant wildcard rules
    public override void ExitJsonPath(JsonPathParser.JsonPathContext ctx)
    {
        var segments = ctx.pathSegment();
        for (int i = 0; i < segments.Length - 1; i++)
        {
            if (segments[i] is JsonPathParser.DescendantSegmentContext dseg &&
                dseg.descendantMemberSegment() is JsonPathParser.WildcardSegmentContext wild)
            {
                Add("InvalidRecursiveWildcard", wild.STAR().Symbol,
                    "Recursive wildcard ('..*') must terminate the path");
            }
        }
    }

    public override void ExitBracketedSelector(JsonPathParser.BracketedSelectorContext ctx)
    {
        var selectors = ctx._selectors;
        if (selectors.Count == 0)
        {
            Add("EmptyUnion", ctx.LBRACKET().Symbol, "Empty union '[]' is not allowed");
        }

        // mixed name and index unions should not occur --> $['a',1]
        bool hasName = selectors.Any(s => s is JsonPathParser.NameSelectorContext);
        bool hasIndex = selectors.Any(s => s is JsonPathParser.IndexSelectorContext);
        bool hasSlice = selectors.Any(s => s is JsonPathParser.SliceSelectorContext);
        
        if (hasName && hasIndex)
        {
            Add("MixedUnion", ctx.LBRACKET().Symbol,
                "Unions cannot mix name and index selectors");
        }
        
        if (hasName && hasSlice)
        {
            Add("MixedUnion", ctx.LBRACKET().Symbol,
                "Unions cannot mix name and slice selectors");
        }        
    }
}
