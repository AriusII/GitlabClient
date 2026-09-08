using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

internal sealed class GitLabContainerRegistryProtectionAccessLevelOrUnsetConverter
    : JsonConverter<GitLabContainerRegistryProtectionAccessLevelOrUnset>
{
    public override GitLabContainerRegistryProtectionAccessLevelOrUnset Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString() switch
        {
            "maintainer" => GitLabContainerRegistryProtectionAccessLevelOrUnset.Maintainer,
            "owner" => GitLabContainerRegistryProtectionAccessLevelOrUnset.Owner,
            "admin" => GitLabContainerRegistryProtectionAccessLevelOrUnset.Admin,
            "" or null => GitLabContainerRegistryProtectionAccessLevelOrUnset.Unset,
            string other => throw new JsonException(
                $"Unrecognized container registry protection access level '{other}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer, GitLabContainerRegistryProtectionAccessLevelOrUnset value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            GitLabContainerRegistryProtectionAccessLevelOrUnset.Maintainer => "maintainer",
            GitLabContainerRegistryProtectionAccessLevelOrUnset.Owner => "owner",
            GitLabContainerRegistryProtectionAccessLevelOrUnset.Admin => "admin",
            GitLabContainerRegistryProtectionAccessLevelOrUnset.Unset => "",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown enum member.")
        });
    }
}