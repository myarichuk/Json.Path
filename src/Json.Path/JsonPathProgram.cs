using System;

namespace Json.Path;

/// <summary>
/// Represents a compiled JSONPath expression.
/// </summary>
public sealed class JsonPathProgram
{
    internal static readonly JsonPathProgram Empty = new(Array.Empty<JsonPathInstruction>());

    private readonly JsonPathInstruction[] _instructions;

    internal JsonPathProgram(JsonPathInstruction[] instructions)
    {
        _instructions = instructions;
    }

    /// <summary>
    /// Evaluates the compiled expression against the provided <paramref name="root"/>.
    /// </summary>
    /// <typeparam name="TNode">The node representation supplied by the navigator.</typeparam>
    /// <param name="root">The root node to resolve the path against.</param>
    /// <param name="navigator">The navigator responsible for resolving properties and array elements.</param>
    /// <param name="result">When the method returns <see langword="true"/>, contains the resolved node.</param>
    /// <returns><see langword="true"/> if the path resolves to a value; otherwise, <see langword="false"/>.</returns>
    public bool TryEvaluate<TNode>(TNode root, IJsonNavigator<TNode> navigator, out TNode result)
    {
        var instructions = _instructions.AsSpan();
        var current = root;

        for (var i = 0; i < instructions.Length; i++)
        {
            ref readonly var instruction = ref instructions[i];
            if (!TryApplyInstruction(instruction, navigator, current, out var next))
            {
                result = default!;
                return false;
            }

            current = next;
        }

        result = current;
        return true;
    }

    private static bool TryApplyInstruction<TNode>(in JsonPathInstruction instruction, IJsonNavigator<TNode> navigator, TNode current, out TNode result)
    {
        switch (instruction.Kind)
        {
            case JsonPathInstructionKind.Property:
                return navigator.TryGetProperty(current, instruction.PropertyName, out result);
            case JsonPathInstructionKind.ArrayIndex:
                return navigator.TryGetElement(current, instruction.ArrayIndex, out result);
            default:
                result = default!;
                return false;
        }
    }
}
