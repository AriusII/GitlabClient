using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Queries;

/// <summary>Variables for a curated <c>workItem(id: …)</c> GraphQL query.</summary>
public sealed record GitLabWorkItemByIdQueryVariables
{
    /// <summary>Initializes query variables for one opaque work-item global ID.</summary>
    /// <param name="id">The target work item's GraphQL global ID.</param>
    /// <exception cref="ArgumentNullException"><paramref name="id" /> is null.</exception>
    public GitLabWorkItemByIdQueryVariables(GitLabGraphQLGlobalId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
    }

    /// <summary>The target work item's opaque GraphQL global ID.</summary>
    [JsonPropertyName("id")]
    public GitLabGraphQLGlobalId Id { get; }
}