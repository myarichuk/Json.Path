using System;

namespace Json.Path;

internal enum JsonPathInstructionKind
{
    Property,
    ArrayIndex,
}

internal readonly struct JsonPathInstruction
{
    private readonly JsonPathInstructionKind _kind;
    private readonly string? _propertyName;
    private readonly int _arrayIndex;

    private JsonPathInstruction(JsonPathInstructionKind kind, string? propertyName, int arrayIndex)
    {
        _kind = kind;
        _propertyName = propertyName;
        _arrayIndex = arrayIndex;
    }

    public JsonPathInstructionKind Kind => _kind;

    public ReadOnlySpan<char> PropertyName => _propertyName.AsSpan();

    public int ArrayIndex => _arrayIndex;

    public static JsonPathInstruction ForProperty(string propertyName)
        => new(JsonPathInstructionKind.Property, propertyName, -1);

    public static JsonPathInstruction ForArrayIndex(int index)
        => new(JsonPathInstructionKind.ArrayIndex, null, index);
}
