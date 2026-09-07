using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Whether instance runners are available to a group's projects, and whether subgroups may
///     override that (<c>shared_runners_setting</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabGroupSharedRunnersSetting>))]
public enum GitLabGroupSharedRunnersSetting
{
    [JsonStringEnumMemberName("enabled")] Enabled,

    /// <summary>Off here, but a subgroup may turn it back on.</summary>
    [JsonStringEnumMemberName("disabled_and_overridable")]
    DisabledAndOverridable,

    /// <summary>Off here and everywhere below.</summary>
    [JsonStringEnumMemberName("disabled_and_unoverridable")]
    DisabledAndUnoverridable
}