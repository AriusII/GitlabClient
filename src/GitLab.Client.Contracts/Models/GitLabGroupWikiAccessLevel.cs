using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Visibility of the group wiki (<c>wiki_access_level</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabGroupWikiAccessLevel>))]
public enum GitLabGroupWikiAccessLevel
{
    [JsonStringEnumMemberName("disabled")] Disabled,

    /// <summary>Visible to group members only.</summary>
    [JsonStringEnumMemberName("private")] Private,

    /// <summary>Visible to everyone who can see the group.</summary>
    [JsonStringEnumMemberName("enabled")] Enabled
}