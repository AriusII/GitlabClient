using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Mutations;

/// <summary>The payload returned by the GitLab GraphQL <c>workItemUpdate</c> mutation.</summary>
/// <remarks>
///     An empty <see cref="Errors" /> collection means GitLab accepted the mutation. A null collection means the
///     operation did not select <c>errors</c>. <see cref="WorkItem" /> is null when GitLab rejected the input.
/// </remarks>
public sealed record GitLabWorkItemUpdatePayload
{
    /// <summary>The updated work item, or null when the mutation reported errors.</summary>
    [JsonPropertyName("workItem")]
    public GitLabWorkItem? WorkItem { get; init; }

    /// <summary>The mutation errors selected by the operation.</summary>
    [JsonPropertyName("errors")]
    public IReadOnlyList<string>? Errors { get; init; }

    /// <summary>The optional caller correlation identifier returned by GraphQL.</summary>
    [JsonPropertyName("clientMutationId")]
    public string? ClientMutationId { get; init; }
}