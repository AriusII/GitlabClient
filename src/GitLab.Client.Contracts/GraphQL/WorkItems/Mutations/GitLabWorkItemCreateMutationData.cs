using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Mutations;

/// <summary>The <c>data</c> object returned by a curated <c>workItemCreate</c> operation.</summary>
public sealed record GitLabWorkItemCreateMutationData
{
    /// <summary>The create payload, or null when GraphQL could not execute the top-level mutation field.</summary>
    [JsonPropertyName("workItemCreate")]
    public GitLabWorkItemCreatePayload? WorkItemCreate { get; init; }
}