using System;
using System.Collections.Generic;
using System.Globalization;

namespace Json.Path;

/// <summary>
/// Provides helpers for compiling and evaluating lightweight JSONPath expressions.
/// </summary>
public static class JsonPath
{
    private const char Dot = '.';
    private const char Dollar = '$';
    private const char ArrayOpen = '[';
    private const char ArrayClose = ']';

    /// <summary>
    /// Attempts to compile the specified <paramref name="path"/> into a reusable instruction sequence.
    /// </summary>
    /// <param name="path">A JSONPath expression starting with <c>$</c>.</param>
    /// <param name="program">When the method returns <see langword="true"/>, contains the compiled path.</param>
    /// <returns><see langword="true"/> if the expression could be compiled; otherwise, <see langword="false"/>.</returns>
    public static bool TryCompile(string path, out JsonPathProgram program)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            program = null!;
            return false;
        }

        return TryCompile(path.AsSpan(), out program);
    }

    /// <summary>
    /// Attempts to resolve the specified <paramref name="path"/> against <paramref name="root"/> using the provided navigator.
    /// </summary>
    /// <typeparam name="TNode">The node representation supplied by the navigator.</typeparam>
    /// <param name="root">The root node to resolve the path against.</param>
    /// <param name="path">A JSONPath expression starting with <c>$</c>.</param>
    /// <param name="navigator">The navigator responsible for resolving properties and array elements.</param>
    /// <param name="result">When the method returns <see langword="true"/>, contains the resolved node.</param>
    /// <returns><see langword="true"/> if the path resolves to a value; otherwise, <see langword="false"/>.</returns>
    public static bool TryResolve<TNode>(TNode root, string path, IJsonNavigator<TNode> navigator, out TNode result)
    {
        if (!TryCompile(path, out var program))
        {
            result = default!;
            return false;
        }

        return program.TryEvaluate(root, navigator, out result);
    }

    private static bool TryCompile(ReadOnlySpan<char> span, out JsonPathProgram program)
    {
        if (span.Length == 0 || span[0] != Dollar)
        {
            program = null!;
            return false;
        }

        if (span.Length == 1)
        {
            program = JsonPathProgram.Empty;
            return true;
        }

        var instructions = new List<JsonPathInstruction>();
        var index = 1;

        while (index < span.Length)
        {
            if (span[index] == Dot)
            {
                index++;
                if (!TryReadPropertySegment(span, ref index, out var propertyName))
                {
                    program = null!;
                    return false;
                }

                instructions.Add(JsonPathInstruction.ForProperty(propertyName));
                continue;
            }

            if (span[index] == ArrayOpen)
            {
                index++;
                if (!TryReadArrayIndex(span, ref index, out var arrayIndex))
                {
                    program = null!;
                    return false;
                }

                instructions.Add(JsonPathInstruction.ForArrayIndex(arrayIndex));
                continue;
            }

            program = null!;
            return false;
        }

        program = instructions.Count == 0
            ? JsonPathProgram.Empty
            : new JsonPathProgram(instructions.ToArray());
        return true;
    }

    private static bool TryReadPropertySegment(ReadOnlySpan<char> span, ref int index, out string propertyName)
    {
        var start = index;
        while (index < span.Length && span[index] is not Dot and not ArrayOpen)
        {
            index++;
        }

        if (start == index)
        {
            propertyName = string.Empty;
            return false;
        }

        propertyName = span[start..index].ToString();
        return true;
    }

    private static bool TryReadArrayIndex(ReadOnlySpan<char> span, ref int index, out int value)
    {
        var start = index;
        while (index < span.Length && span[index] != ArrayClose)
        {
            index++;
        }

        if (index >= span.Length || span[index] != ArrayClose)
        {
            value = -1;
            return false;
        }

        var slice = span[start..index];
        index++; // skip closing bracket

        if (!int.TryParse(slice, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) || value < 0)
        {
            value = -1;
            return false;
        }

        return true;
    }
}
