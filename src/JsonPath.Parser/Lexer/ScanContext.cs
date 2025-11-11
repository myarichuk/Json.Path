using System;

namespace JsonPath.Parser.Lexer;

/// <summary>
/// Maintains the state of a scanning operation over a JsonPath input span.
/// </summary>
public ref struct ScanContext(ReadOnlySpan<char> input)
{
    /// <summary>
    /// Gets the source characters being scanned.
    /// </summary>
    public readonly ReadOnlySpan<char> Input = input;

    /// <summary>
    /// Gets or sets the current offset into <see cref="Input"/>.
    /// </summary>
    public int Position;

    /// <summary>
    /// Gets the current character or <c>'\0'</c> when the end of input is reached.
    /// </summary>
    public char Current =>
        Position < Input.Length ? Input[Position] : '\0';

    /// <summary>
    /// Gets the number of characters remaining in the input.
    /// </summary>
    public int RemainingLength => Input.Length - Position;

    /// <summary>
    /// Peeks ahead without advancing the current position.
    /// </summary>
    /// <param name="offset">The lookahead offset relative to the current position.</param>
    /// <returns>The character at the lookahead offset, or <c>'\0'</c> when beyond input.</returns>
    public char Peek(int offset = 1) =>
        Position + offset < Input.Length ? Input[Position + offset] : '\0';

    /// <summary>
    /// Advances the current position by the specified count.
    /// </summary>
    /// <param name="count">The number of characters to consume.</param>
    public void Consume(int count = 1)
    {
        Position = Math.Min(Input.Length, Position + Math.Max(0, count));
    }

    /// <summary>
    /// Gets the unread portion of the input.
    /// </summary>
    public ReadOnlySpan<char> RemainingInput => Input[Position..];

    /// <summary>
    /// Slices the input starting at the current position offset by <paramref name="offset"/>.
    /// </summary>
    /// <param name="offset">The number of characters to skip from <see cref="Position"/>.</param>
    /// <returns>A slice of the input span.</returns>
    public ReadOnlySpan<char> SliceOffset(int offset) => Input[(Position + offset)..];

    /// <summary>
    /// Retrieves the span represented by a token.
    /// </summary>
    /// <param name="token">The token describing the slice.</param>
    /// <returns>The span of characters backing the token.</returns>
    public ReadOnlySpan<char> SliceFrom(in Token token) => Input.Slice(token.Start, token.Length);
}