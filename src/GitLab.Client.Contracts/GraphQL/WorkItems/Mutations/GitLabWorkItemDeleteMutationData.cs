using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Mutations;

/// <summary>The <c>data</c> object returned by a curated <c>workItemDelete</c> operation.</summary>
public sealed record GitLabWorkItemDeleteMutationData
{
    /// <summary>The delete payload, or null when GraphQL could not execute the top-level mutation field.</summary>
    [JsonPropertyName("workItemDelete")]
    public GitLabWorkItemDeletePayload? WorkItemDelete { get; init; }
}