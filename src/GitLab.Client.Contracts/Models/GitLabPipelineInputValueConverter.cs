using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

internal sealed class GitLabPipelineInputValueConverter : JsonConverter<GitLabPipelineInputValue>
{
    public override GitLabPipelineInputValue Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        return ReadValue(ref reader);
    }

    private static GitLabPipelineInputValue ReadValue(ref Utf8JsonReader reader)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => GitLabPipelineInputValue.Null(),
            JsonTokenType.String => ReadString(ref reader),
            JsonTokenType.Number => GitLabPipelineInputValue.Number(reader.GetDouble()),
            JsonTokenType.True => GitLabPipelineInputValue.Flag(true),
            JsonTokenType.False => GitLabPipelineInputValue.Flag(false),
            JsonTokenType.StartArray => ReadArray(ref reader),
            _ => throw new JsonException($"Unsupported pipeline input JSON token '{reader.TokenType}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer, GitLabPipelineInputValue value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);

        switch (value.Kind)
        {
            case GitLabPipelineInputValueKind.Null:
                writer.WriteNullValue();
                break;
            case GitLabPipelineInputValueKind.Text:
                writer.WriteStringValue(value.TextValue);
                break;
            case GitLabPipelineInputValueKind.Number:
                writer.WriteNumberValue(value.NumberValue.GetValueOrDefault());
                break;
            case GitLabPipelineInputValueKind.Flag:
                writer.WriteBooleanValue(value.FlagValue.GetValueOrDefault());
                break;
            case GitLabPipelineInputValueKind.Sequence:
                writer.WriteStartArray();
                foreach (GitLabPipelineInputValue item in value.SequenceValue ?? [])
                {
                    Write(writer, item, options);
                }

                writer.WriteEndArray();
                break;
            default:
                throw new JsonException($"Unsupported pipeline input value kind '{value.Kind}'.");
        }
    }

    private static GitLabPipelineInputValue ReadString(ref Utf8JsonReader reader)
    {
        return reader.GetString() is { } value
            ? GitLabPipelineInputValue.Text(value)
            : throw new JsonException("A JSON string input value cannot be null.");
    }

    private static GitLabPipelineInputValue ReadArray(ref Utf8JsonReader reader)
    {
        List<GitLabPipelineInputValue> values = [];
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            values.Add(ReadValue(ref reader));
        }

        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Unexpected end of JSON while reading a pipeline input array.");
        }

        return GitLabPipelineInputValue.Sequence(values);
    }
}