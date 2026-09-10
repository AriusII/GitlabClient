using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Identifiers;

/// <summary>Serializes <see cref="GitLabGraphQLGlobalId" /> as GitLab's scalar GraphQL ID string.</summary>
internal sealed class GitLabGraphQLGlobalIdJsonConverter : JsonConverter<GitLabGraphQLGlobalId>
{
    public override GitLabGraphQLGlobalId Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("A GitLab GraphQL global ID must be a JSON string.");
        }

        return reader.GetString() is { Length: > 0 } value
            ? new GitLabGraphQLGlobalId(value)
            : throw new JsonException("A GitLab GraphQL global ID cannot be empty.");
    }

    public override void Write(Utf8JsonWriter writer, GitLabGraphQLGlobalId value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);
        writer.WriteStringValue(value.Value);
    }
}