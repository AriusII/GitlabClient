using System.Collections.ObjectModel;
using System.Text.Json;

namespace GitLab.Client.Models.Responses;

/// <summary>
///     GitLab's error body shape. <c>message</c> varies between a plain string and a structured
///     validation-error object depending on the endpoint, so it is captured as a raw <see cref="JsonElement" />.
/// </summary>
public sealed record GitLabErrorResponse
{
    public JsonElement? Message { get; init; }

    public string? Error { get; init; }

    public string? ErrorDescription { get; init; }

    public string? ToDisplayMessage()
    {
        return Message is { } message
            ? message.ValueKind == JsonValueKind.String ? message.GetString() : message.GetRawText()
            : Error ?? ErrorDescription;
    }

    /// <summary>
    ///     GitLab's per-field validation errors, read out of the structured form of <c>message</c>
    ///     (<c>{"message":{"title":["can't be blank"]}}</c>). Empty when <c>message</c> was a plain string or
    ///     absent.
    /// </summary>
    /// <returns>Field name to the messages GitLab reported for it; empty, never null, when there are none.</returns>
    /// <remarks>
    ///     Walks the <see cref="JsonElement" /> directly rather than deserializing into a dictionary type, so it
    ///     needs no <see cref="System.Text.Json.Serialization.Metadata.JsonTypeInfo" /> and stays trim- and
    ///     Native-AOT-safe by construction.
    /// </remarks>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> ToValidationErrors()
    {
        if (Message is not { } message || message.ValueKind != JsonValueKind.Object)
        {
            return ReadOnlyDictionary<string, IReadOnlyList<string>>.Empty;
        }

        Dictionary<string, IReadOnlyList<string>> errors = new(StringComparer.Ordinal);

        foreach (JsonProperty property in message.EnumerateObject())
        {
            errors[property.Name] = ReadMessages(property.Value);
        }

        return errors.Count == 0
            ? ReadOnlyDictionary<string, IReadOnlyList<string>>.Empty
            : new ReadOnlyDictionary<string, IReadOnlyList<string>>(errors);
    }

    private static string[] ReadMessages(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            return [ReadText(value)];
        }

        string[] messages = new string[value.GetArrayLength()];
        int index = 0;

        foreach (JsonElement item in value.EnumerateArray())
        {
            messages[index] = ReadText(item);
            index++;
        }

        return messages;
    }

    private static string ReadText(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.String
            ? element.GetString() ?? string.Empty
            : element.GetRawText();
    }
}