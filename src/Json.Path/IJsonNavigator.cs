using System;

namespace Json.Path;

/// <summary>
/// Represents the operations required to navigate JSON-like structures for JSONPath evaluation.
/// </summary>
/// <typeparam name="TNode">The node representation supplied by a serializer or DOM.</typeparam>
public interface IJsonNavigator<TNode>
{
    /// <summary>
    /// Attempts to resolve a property on the specified <paramref name="node"/>.
    /// </summary>
    /// <param name="node">The object node to inspect.</param>
    /// <param name="propertyName">The property name to resolve.</param>
    /// <param name="value">When the method returns <see langword="true"/>, contains the resolved value.</param>
    /// <returns><see langword="true"/> if the property could be resolved; otherwise, <see langword="false"/>.</returns>
    bool TryGetProperty(TNode node, ReadOnlySpan<char> propertyName, out TNode value);

    /// <summary>
    /// Attempts to resolve an array element on the specified <paramref name="node"/>.
    /// </summary>
    /// <param name="node">The array node to inspect.</param>
    /// <param name="index">The zero-based array index.</param>
    /// <param name="value">When the method returns <see langword="true"/>, contains the resolved value.</param>
    /// <returns><see langword="true"/> if the array element could be resolved; otherwise, <see langword="false"/>.</returns>
    bool TryGetElement(TNode node, int index, out TNode value);
}
