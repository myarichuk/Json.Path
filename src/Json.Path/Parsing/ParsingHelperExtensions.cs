using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

// ReSharper disable UnusedType.Global

namespace Json.Path.Parsing
{
    /// <summary>
    /// Provides helper extension methods for navigating ANTLR parse trees.
    /// </summary>
    /// <remarks>
    /// These helpers make it easier to search a <see cref="ParserRuleContext"/> for specific tokens.
    /// In ANTLR, a <see cref="ParserRuleContext"/> represents a grammar rule node in the parse tree,
    /// and an <see cref="ITerminalNode"/> represents an actual token (a leaf in the tree).
    /// </remarks>
    public static class ParsingHelperExtensions
    {
        /// <summary>
        /// Returns all immediate child <see cref="ITerminalNode"/> instances of the given context
        /// that match a specific token type.
        /// </summary>
        /// <param name="ctx">The current <see cref="ParserRuleContext"/> node to inspect.</param>
        /// <param name="tokenType">
        /// The token type ID to match (usually a constant defined in the generated lexer class,
        /// e.g., <c>JsonPathLexer.DOT</c> or <c>JsonPathLexer.STAR</c>).
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ITerminalNode"/> objects that are *direct* children of the context
        /// and whose token type matches <paramref name="tokenType"/>.
        /// Returns an empty sequence if there are no matching terminals.
        /// </returns>
        /// <remarks>
        /// This method does not recurse into nested rules; it only checks the context’s
        /// immediate children. Use <see cref="EnumerateDescendantTerminalsOfType"/> to perform a recursive search.
        /// <para/>
        /// Also note:
        /// <list type="bullet">
        /// <item><description><b>Token</b> – pre-tree, produced by the lexer and stored in an <see cref="ITokenStream"/>.</description></item>
        /// <item><description><b>Terminal</b> – in-tree node that wraps an <see cref="IToken"/> (leaf of the parse tree).</description></item>
        /// <item><description><b>Rule context</b> – non-terminal node that represents a grammar rule (internal node of the tree).</description></item>
        /// </list>
        /// </remarks>
        public static IEnumerable<ITerminalNode> EnumerateTerminalsOfType(this ParserRuleContext ctx, int tokenType) =>
            ctx.children?
                .OfType<ITerminalNode>()
                .Where(t => t.Symbol.Type == tokenType)
            ?? [];

        /// <summary>
        /// Recursively enumerates all <see cref="ITerminalNode"/> instances within the given context
        /// and its descendant subtrees that match a specific token type.
        /// </summary>
        /// <param name="ctx">The current <see cref="ParserRuleContext"/> node to inspect.</param>
        /// <param name="tokenType">
        /// The token type ID to match (usually a constant from the generated lexer).
        /// </param>
        /// <returns>
        /// A depth-first enumeration of all matching <see cref="ITerminalNode"/> objects found anywhere
        /// within <paramref name="ctx"/> and its descendants.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method performs a recursive traversal of the entire subtree.
        /// For performance-sensitive checks where only immediate tokens are relevant,
        /// prefer <see cref="EnumerateTerminalsOfType"/>.
        /// </para>
        /// Also note:
        /// <list type="bullet">
        /// <item><description><b>Token</b> – pre-tree, produced by the lexer and stored in an <see cref="ITokenStream"/>.</description></item>
        /// <item><description><b>Terminal</b> – in-tree node that wraps an <see cref="IToken"/> (leaf of the parse tree).</description></item>
        /// <item><description><b>Rule context</b> – non-terminal node that represents a grammar rule (internal node of the tree).</description></item>
        /// </list>
        /// </remarks>
        public static IEnumerable<ITerminalNode> EnumerateDescendantTerminalsOfType(this ParserRuleContext ctx, int tokenType)
        {
            foreach (var child in ctx.children ?? Enumerable.Empty<IParseTree>())
            {
                if (child is ITerminalNode term && term.Symbol.Type == tokenType)
                    yield return term;
                else if (child is ParserRuleContext rule)
                {
                    foreach (var inner in rule.EnumerateDescendantTerminalsOfType(tokenType))
                        yield return inner;
                }
            }
        }
    }
}
