using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The severity accepted by the issue create/update request bodies. GitLab only applies it to
///     incidents.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabIssueSeverity>))]
public enum GitLabIssueSeverity
{
    [JsonStringEnumMemberName("unknown")] Unknown,

    [JsonStringEnumMemberName("low")] Low,

    [JsonStringEnumMemberName("medium")] Medium,

    [JsonStringEnumMemberName("high")] High,

    [JsonStringEnumMemberName("critical")] Critical
}