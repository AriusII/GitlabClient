using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Queries;

/// <summary>Data returned by a curated <c>workItem(id: …)</c> GraphQL query.</summary>
public sealed record GitLabWorkItemByIdQueryData
{
    /// <summary>
    ///     The requested work item, or null when it does not exist or GitLab intentionally hides it from the caller.
    /// </summary>
    [JsonPropertyName("workItem")]
    public GitLabWorkItem? WorkItem { get; init; }
}