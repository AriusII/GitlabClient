using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Models;

/// <summary>The documented relative position used by a hierarchy-widget update.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabWorkItemRelativePosition>))]
public enum GitLabWorkItemRelativePosition
{
    /// <summary>Place the work item before the adjacent work item.</summary>
    [JsonStringEnumMemberName("BEFORE")] Before,

    /// <summary>Place the work item after the adjacent work item.</summary>
    [JsonStringEnumMemberName("AFTER")] After
}