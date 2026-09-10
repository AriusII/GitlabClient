using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Queries;

namespace GitLab.Client.GraphQL.WorkItems.Mutations;

/// <summary>The payload returned by the GitLab GraphQL <c>workItemDelete</c> mutation.</summary>
/// <remarks>
///     An empty <see cref="Errors" /> collection means GitLab accepted the mutation. A null collection means the
///     operation did not select <c>errors</c>. The namespace is null when GitLab rejected the deletion or it was not
///     selected by the operation.
/// </remarks>
public sealed record GitLabWorkItemDeletePayload
{
    /// <summary>The namespace that contained the deleted work item, when GitLab returns it.</summary>
    [JsonPropertyName("namespace")]
    public GitLabWorkItemNamespace? Namespace { get; init; }

    /// <summary>The mutation errors selected by the operation.</summary>
    [JsonPropertyName("errors")]
    public IReadOnlyList<string>? Errors { get; init; }

    /// <summary>The optional caller correlation identifier returned by GraphQL.</summary>
    [JsonPropertyName("clientMutationId")]
    public string? ClientMutationId { get; init; }
}