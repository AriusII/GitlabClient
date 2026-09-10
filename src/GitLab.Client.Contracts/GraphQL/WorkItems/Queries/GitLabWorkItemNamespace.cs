using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Connections;
using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Queries;

/// <summary>The minimal namespace projection used by the curated work-item queries and delete payload.</summary>
/// <remarks>
///     Its work-item members are nullable because every operation selects a different subset. A null member means it
///     was omitted from that operation, is unavailable on the namespace, or is intentionally hidden by GitLab's
///     authorization model; it must not be interpreted as an empty connection.
/// </remarks>
public sealed record GitLabWorkItemNamespace
{
    /// <summary>The namespace's opaque GraphQL global ID, when selected.</summary>
    [JsonPropertyName("id")]
    public GitLabGraphQLGlobalId? Id { get; init; }

    /// <summary>The namespace's full path, when selected.</summary>
    [JsonPropertyName("fullPath")]
    public string? FullPath { get; init; }

    /// <summary>The work item resolved by an IID locator, when selected and visible.</summary>
    [JsonPropertyName("workItem")]
    public GitLabWorkItem? WorkItem { get; init; }

    /// <summary>The cursor-paginated namespace work-item list, when selected.</summary>
    [JsonPropertyName("workItems")]
    public GitLabWorkItemConnection? WorkItems { get; init; }

    /// <summary>The cursor-paginated namespace work-item-type list, when selected.</summary>
    [JsonPropertyName("workItemTypes")]
    public GitLabWorkItemTypeConnection? WorkItemTypes { get; init; }
}