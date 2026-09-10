using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Mutations;

/// <summary>The input for the GitLab GraphQL <c>workItemDelete</c> mutation.</summary>
public sealed record GitLabWorkItemDeleteInput
{
    /// <summary>Initializes a deletion for one opaque work-item global ID.</summary>
    /// <param name="id">The target work item's GraphQL global ID.</param>
    /// <exception cref="ArgumentNullException"><paramref name="id" /> is null.</exception>
    public GitLabWorkItemDeleteInput(GitLabGraphQLGlobalId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
    }

    /// <summary>The opaque GraphQL global ID of the work item to delete.</summary>
    [JsonPropertyName("id")]
    public GitLabGraphQLGlobalId Id { get; }
}