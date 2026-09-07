using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Who may create projects inside a group (<c>project_creation_level</c>).
///     <para>
///         Only sent, never read back: the group entity types the field as a bare string, so
///         <see cref="GitLabGroup.ProjectCreationLevel" /> stays a string and a value GitLab adds later
///         cannot turn a healthy response into a <c>JsonException</c>.
///     </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabGroupProjectCreationLevel>))]
public enum GitLabGroupProjectCreationLevel
{
    /// <summary>Nobody, not even owners.</summary>
    [JsonStringEnumMemberName("noone")] NoOne,

    [JsonStringEnumMemberName("owner")] Owner,

    [JsonStringEnumMemberName("maintainer")]
    Maintainer,

    [JsonStringEnumMemberName("developer")]
    Developer,

    /// <summary>Instance administrators only.</summary>
    [JsonStringEnumMemberName("administrator")]
    Administrator
}