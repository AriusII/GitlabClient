using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Who may create subgroups inside a group (<c>subgroup_creation_level</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabGroupSubgroupCreationLevel>))]
public enum GitLabGroupSubgroupCreationLevel
{
    [JsonStringEnumMemberName("owner")] Owner,

    [JsonStringEnumMemberName("maintainer")]
    Maintainer
}