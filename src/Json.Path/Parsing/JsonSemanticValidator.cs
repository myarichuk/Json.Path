#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
// ReSharper disable PossibleMultipleEnumeration

namespace Json.Path.Parsing;

public record struct SemanticError(string Code, string Message, Interval Span);

// A listener that inspects the parse tree and uses the token stream to check RFC constraints
public class JsonPathSemanticValidator(CommonTokenStream tokens) : JsonPathBaseListener
{
    private readonly List<SemanticError> _errors = new();

    public IReadOnlyList<SemanticError> Errors => _errors;

    #region Helpers
    
    private bool HasHiddenWsAround(IToken t)
    {
        if (t.TokenIndex < 0 || t.TokenIndex >= tokens.Size)
        {
            return false;
        }
        
        var left  = tokens.GetHiddenTokensToLeft(t.TokenIndex);
        var right = tokens.GetHiddenTokensToRight(t.TokenIndex);
        return left is { Count: > 0 } || right is { Count: > 0 };
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

    public override void ExitJsonPath(JsonPathParser.JsonPathContext ctx)
    {
        var segments = ctx.pathSegment();
        for (var i = 0; i < segments.Length - 1; i++)
        {
            if (segments[i] is JsonPathParser.DescendantSegmentContext descendant)
            {
                var memberSegment = descendant.descendantMemberSegment();

                if (memberSegment is JsonPathParser.WildcardSegmentContext wildcardSegment && i != segments.Length - 1)
                {
                    var nextSegment = segments[i + 1];
                    if (nextSegment is JsonPathParser.DescendantSegmentContext)
                    {
                        Add("InvalidRecursiveWildcard", wildcardSegment.STAR().Symbol,
                            "Recursive wildcard (`..*`) cannot be followed by another recursive descent segment");                        
                    }
                    else if (nextSegment is JsonPathParser.ChildSegmentContext nextChildSegment &&
                             nextChildSegment.memberSegment() is not JsonPathParser.BracketedChildSelectionContext and not 
                                 JsonPathParser.QueryChildSelectionContext)
                    {
                        Add("InvalidRecursiveWildcard", wildcardSegment.STAR().Symbol,
                            "Recursive wildcard (`..*`) cannot be followed by anything other than array indexer or a query segment");                         
                    }
                }
            }
        }
    }

    public override void ExitSliceSelector(JsonPathParser.SliceSelectorContext ctx)
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
            Add("SliceZeroStep", ctx.step, "Slice step must be non-zero");
        }
    }
    
    
}
