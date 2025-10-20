#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Json.Path.Parsing;

public record struct SemanticError(string Code, string Message, Interval Span);

// A listener that inspects the parse tree and uses the token stream to check RFC constraints
public sealed class JsonPathSemanticValidator(CommonTokenStream tokens) : JsonPathBaseListener
{
    private readonly List<SemanticError> _errors = new();

    public IReadOnlyList<SemanticError> Errors => _errors;

    #region Helpers
    
    private static ITerminalNode[] Terminals(ParserRuleContext ctx, int tokenType) =>
        ctx.children?
           .OfType<ITerminalNode>()
           .Where(t => t.Symbol.Type == tokenType)
           .ToArray()
        ?? [];

    private bool HasHiddenWsAround(IToken t)
    {
        var left  = tokens.GetHiddenTokensToLeft(t.TokenIndex);
        var right = tokens.GetHiddenTokensToRight(t.TokenIndex);
        return (left is { Count: > 0 }) || (right is { Count: > 0 });
    }

    private void Add(string code, IToken tok, string message)
    {
        var start = tok.StartIndex;
        var stop  = tok.StopIndex;
        _errors.Add(new SemanticError(code, message, new Interval(start, stop)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsZero(IToken? integerToken) =>
        integerToken?.Text is "0" or "+0" or "-0";

    #endregion
    
    #region Slices
 
    public override void ExitSlice([NotNull] JsonPathParser.SliceContext ctx)
    {
        // no whitespace around any slice tokens (numbers and colons)
        void CheckNoWs(IToken? tok)
        {
            if (tok is null)
            {
                return;
            }

            if (HasHiddenWsAround(tok))
            {
                Add("SliceWhitespace", tok, "Whitespace inside slice is not allowed (RFC 9535 §2.6).");
            }
        }

        CheckNoWs(ctx.startIndex);
        CheckNoWs(ctx.c1);
        CheckNoWs(ctx.endIndex);
        CheckNoWs(ctx.c2);
        CheckNoWs(ctx.step);

        // step != 0
        if (ctx.step is { Text: "0" or "+0" or "-0" })
        {
            Add("SliceZeroStep", ctx.step, "Slice step must be non-zero (RFC 9535 §2.6).");
        }
    }

    #endregion

    #region Recursive Descent
    public override void ExitRecursive([NotNull] JsonPathParser.RecursiveContext ctx)
    {
        // recursive descent segment must have ONE selector -> $..*['a'] is invalid
        var selectors = ctx.indexer();
        if (selectors is { Length: > 1 })
        {
            Add("RecursiveDescentTooManySelectors",
                selectors[1].Start,
                "Recursive descent segment must have exactly one selector.");
        }

        // forbid $..*[*] or $..*[1:2]
        if (selectors.Any(s => s.children != null))
        {
            // Add("RecursiveDescentSliceNotAllowed",
            //     selectors.First(s => s.() != null).Start,
            //     "Slicing a recursive-descent match is not allowed.");
        }
    }
    
    #endregion

    #region Union Selector Validators
    
    // TODO: make sure
    // * No wildcard inside unions: $[*,1]
    // * No trailing/leading commas or empty elements
    
    public override void ExitUnionIndex(JsonPathParser.UnionIndexContext ctx)
    {

        if (ctx.GetTokens(JsonPathLexer.STAR).Length > 0)
        {
            Add("UnionWildcardNotAllowed",
                ctx.GetTokens(JsonPathLexer.STAR)[0].Symbol,
                "Wildcard not allowed inside union.");
        }
    }

    #endregion

    #region Index Constrains

    public override void ExitNumericIndex([NotNull] JsonPathParser.NumericIndexContext ctx)
    {
        if (ctx.index is null)
        {
            return;
        }

        // disallow negative indexes for single indexers (keep them for slices/unions).
        if (ctx.index.Text.StartsWith('-'))
        {
            Add("NegativeIndexNotAllowed", ctx.index, "Negative array indices are not allowed in single indexers.");
        }
    }

    
    #endregion
    
    public override void ExitJsonPath([NotNull] JsonPathParser.JsonPathContext ctx)
    {
        var segments = ctx.segment();
        for (int i = 0; i < segments.Length - 1; i++)
        {
            if (segments[i]     is JsonPathParser.RecursiveWildcardContext &&
                segments[i + 1] is JsonPathParser.PropertyBracketedContext next)
            {
                Add("RecursiveWildcardProperty",
                    next.Start,
                    "Cannot apply a bracketed property directly after recursive wildcard ('..*').");
            }
        }
    }    
}
