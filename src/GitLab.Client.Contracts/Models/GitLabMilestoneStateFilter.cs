using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>state</c> filter accepted by project and group milestone listings.
///     This query vocabulary is distinct from the read-only string returned by
///     <see cref="GitLabMilestone.State" /> so a future response state does not make deserialization fail.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabMilestoneStateFilter>))]
public enum GitLabMilestoneStateFilter
{
    /// <summary>Only open milestones.</summary>
    [JsonStringEnumMemberName("active")] Active,

    /// <summary>Only closed milestones.</summary>
    [JsonStringEnumMemberName("closed")] Closed,

    /// <summary>Milestones in every state.</summary>
    [JsonStringEnumMemberName("all")] All
}