using System.Globalization;
using System.Text.Json;

namespace Json.Path;

/// <summary>
/// Provides helpers for resolving lightweight JSONPath expressions against <see cref="JsonElement"/> values.
/// The implementation purposefully focuses on simple scenarios so that applications can plug in richer implementations later.
/// </summary>
public static class JsonPath
{
    private const char Dot = '.';
    private const char Dollar = '$';
    private const char ArrayOpen = '[';
    private const char ArrayClose = ']';

    /// <summary>
    /// Attempts to resolve the specified <paramref name="path"/> against <paramref name="root"/>.
    /// Supports dot separated object access (<c>$.store.book</c>) and integer array indexes (<c>$.items[0]</c>).
    /// </summary>
    /// <param name="root">The JSON element to resolve the path against.</param>
    /// <param name="path">A JSONPath expression starting with <c>$</c>.</param>
    /// <param name="result">When the method returns <see langword="true"/>, contains the resolved element.</param>
    /// <returns><see langword="true"/> if the path resolves to a value; otherwise, <see langword="false"/>.</returns>
    public static bool TryResolve(JsonElement root, string path, out JsonElement result)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            result = default;
            return false;
        }

        var current = root;
        var span = path.AsSpan();
        var index = 0;

        if (span[index] != Dollar)
        {
            result = default;
            return false;
        }

        index++;
        if (index == span.Length)
        {
            result = current;
            return true;
        }

        while (index < span.Length)
        {
            if (span[index] == Dot)
            {
                index++;
                if (!TryReadPropertySegment(span, ref index, out var propertyName))
                {
                    result = default;
                    return false;
                }

                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(propertyName, out current))
                {
                    result = default;
                    return false;
                }

                continue;
            }

            if (span[index] == ArrayOpen)
            {
                index++;
                if (!TryReadArrayIndex(span, ref index, out var arrayIndex))
                {
                    result = default;
                    return false;
                }

                if (!TryResolveArrayIndex(current, arrayIndex, out current))
                {
                    result = default;
                    return false;
                }

                continue;
            }

            result = default;
            return false;
        }

        result = current;
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

    private static bool TryResolveArrayIndex(JsonElement current, int index, out JsonElement resolved)
    {
        if (current.ValueKind != JsonValueKind.Array)
        {
            resolved = default;
            return false;
        }

        var enumerator = current.EnumerateArray();
        var i = 0;
        foreach (var element in enumerator)
        {
            if (i == index)
            {
                resolved = element;
                return true;
            }

            i++;
        }

        resolved = default;
        return false;
    }
}
