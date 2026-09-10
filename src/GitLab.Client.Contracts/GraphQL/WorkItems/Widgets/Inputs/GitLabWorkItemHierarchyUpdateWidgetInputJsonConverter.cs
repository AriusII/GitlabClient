using System.Text.Json;
using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

/// <summary>
///     Source-generation-safe converter that preserves omitted, assigned, and explicitly cleared hierarchy parents.
/// </summary>
internal sealed class GitLabWorkItemHierarchyUpdateWidgetInputJsonConverter
    : JsonConverter<GitLabWorkItemHierarchyUpdateWidgetInput>
{
    public override GitLabWorkItemHierarchyUpdateWidgetInput Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("A hierarchy widget input must be a JSON object.");
        }

        GitLabGraphQLGlobalId? parentId = null;
        bool removeParent = false;
        IReadOnlyList<GitLabGraphQLGlobalId>? childrenIds = null;
        GitLabGraphQLGlobalId? adjacentWorkItemId = null;
        GitLabWorkItemRelativePosition? relativePosition = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("A hierarchy widget input property name was expected.");
            }

            string propertyName = reader.GetString() ?? throw new JsonException("A property name cannot be null.");
            if (!reader.Read())
            {
                throw new JsonException("Unexpected end of JSON while reading a hierarchy widget input.");
            }

            switch (propertyName)
            {
                case "parentId":
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        parentId = null;
                        removeParent = true;
                    }
                    else
                    {
                        parentId = ReadGlobalId(ref reader, propertyName);
                        removeParent = false;
                    }

                    break;
                case "childrenIds":
                    childrenIds = ReadGlobalIds(ref reader, propertyName);
                    break;
                case "adjacentWorkItemId":
                    adjacentWorkItemId = ReadGlobalId(ref reader, propertyName);
                    break;
                case "relativePosition":
                    relativePosition = ReadRelativePosition(ref reader);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        if (reader.TokenType != JsonTokenType.EndObject)
        {
            throw new JsonException("Unexpected end of JSON while reading a hierarchy widget input.");
        }

        return new GitLabWorkItemHierarchyUpdateWidgetInput
        {
            ParentId = parentId,
            RemoveParent = removeParent,
            ChildrenIds = childrenIds,
            AdjacentWorkItemId = adjacentWorkItemId,
            RelativePosition = relativePosition
        };
    }

    public override void Write(Utf8JsonWriter writer, GitLabWorkItemHierarchyUpdateWidgetInput value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.ParentId is not null && value.RemoveParent)
        {
            throw new InvalidOperationException("A hierarchy update cannot assign and remove its parent at once.");
        }

        writer.WriteStartObject();

        if (value.ParentId is not null)
        {
            writer.WriteString("parentId", value.ParentId.Value);
        }
        else if (value.RemoveParent)
        {
            writer.WriteNull("parentId");
        }

        if (value.ChildrenIds is not null)
        {
            writer.WritePropertyName("childrenIds");
            writer.WriteStartArray();
            foreach (GitLabGraphQLGlobalId childId in value.ChildrenIds)
            {
                ArgumentNullException.ThrowIfNull(childId);
                writer.WriteStringValue(childId.Value);
            }

            writer.WriteEndArray();
        }

        if (value.AdjacentWorkItemId is not null)
        {
            writer.WriteString("adjacentWorkItemId", value.AdjacentWorkItemId.Value);
        }

        if (value.RelativePosition is { } position)
        {
            writer.WriteString("relativePosition", position switch
            {
                GitLabWorkItemRelativePosition.Before => "BEFORE",
                GitLabWorkItemRelativePosition.After => "AFTER",
                _ => throw new JsonException("An unknown hierarchy relative position was supplied.")
            });
        }

        writer.WriteEndObject();
    }

    private static GitLabGraphQLGlobalId ReadGlobalId(ref Utf8JsonReader reader, string propertyName)
    {
        if (reader.TokenType != JsonTokenType.String || reader.GetString() is not { Length: > 0 } value)
        {
            throw new JsonException($"'{propertyName}' must be a non-empty GraphQL global ID string.");
        }

        return new GitLabGraphQLGlobalId(value);
    }

    private static List<GitLabGraphQLGlobalId> ReadGlobalIds(ref Utf8JsonReader reader, string propertyName)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"'{propertyName}' must be an array of GraphQL global ID strings.");
        }

        List<GitLabGraphQLGlobalId> ids = [];
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            ids.Add(ReadGlobalId(ref reader, propertyName));
        }

        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException($"Unexpected end of JSON while reading '{propertyName}'.");
        }

        return ids;
    }

    private static GitLabWorkItemRelativePosition ReadRelativePosition(ref Utf8JsonReader reader)
    {
        return reader.TokenType == JsonTokenType.String
            ? reader.GetString() switch
            {
                "BEFORE" => GitLabWorkItemRelativePosition.Before,
                "AFTER" => GitLabWorkItemRelativePosition.After,
                _ => throw new JsonException("An unknown hierarchy relative position was received.")
            }
            : throw new JsonException("'relativePosition' must be a JSON string.");
    }
}