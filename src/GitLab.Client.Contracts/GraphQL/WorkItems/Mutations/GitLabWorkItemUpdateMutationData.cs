using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Mutations;

/// <summary>The <c>data</c> object returned by a curated <c>workItemUpdate</c> operation.</summary>
public sealed record GitLabWorkItemUpdateMutationData
{
    /// <summary>The update payload, or null when GraphQL could not execute the top-level mutation field.</summary>
    [JsonPropertyName("workItemUpdate")]
    public GitLabWorkItemUpdatePayload? WorkItemUpdate { get; init; }
}