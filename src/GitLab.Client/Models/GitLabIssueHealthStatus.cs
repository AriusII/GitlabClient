using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>health_status</c> filter of the issue listing and statistics endpoints.
///     <para>
///         Deliberately not used for <see cref="GitLabIssue.HealthStatus" />: the spec types that response
///         field as a bare string, and <see cref="None" /> and <see cref="Any" /> are filter-only values
///         that never come back on an issue.
///     </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabIssueHealthStatus>))]
public enum GitLabIssueHealthStatus
{
    [JsonStringEnumMemberName("on_track")] OnTrack,

    [JsonStringEnumMemberName("needs_attention")]
    NeedsAttention,

    [JsonStringEnumMemberName("at_risk")] AtRisk,

    /// <summary>Matches issues with no health status set. Filter-only.</summary>
    [JsonStringEnumMemberName("none")] None,

    /// <summary>Matches issues with any health status set. Filter-only.</summary>
    [JsonStringEnumMemberName("any")] Any
}