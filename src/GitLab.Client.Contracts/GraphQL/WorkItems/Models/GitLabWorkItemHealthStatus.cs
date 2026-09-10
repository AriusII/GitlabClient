using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Models;

/// <summary>The documented values of a work item's health-status widget.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabWorkItemHealthStatus>))]
public enum GitLabWorkItemHealthStatus
{
    /// <summary>The work item is progressing as planned.</summary>
    [JsonStringEnumMemberName("onTrack")] OnTrack,

    /// <summary>The work item needs attention.</summary>
    [JsonStringEnumMemberName("needsAttention")]
    NeedsAttention,

    /// <summary>The work item is at risk.</summary>
    [JsonStringEnumMemberName("atRisk")] AtRisk
}