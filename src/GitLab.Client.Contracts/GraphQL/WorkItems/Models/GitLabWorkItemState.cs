using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Models;

/// <summary>The lifecycle state emitted by the GitLab GraphQL <c>WorkItem.state</c> field.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabWorkItemState>))]
public enum GitLabWorkItemState
{
    /// <summary>The work item is open.</summary>
    [JsonStringEnumMemberName("OPEN")] Open,

    /// <summary>The work item is closed.</summary>
    [JsonStringEnumMemberName("CLOSED")] Closed
}