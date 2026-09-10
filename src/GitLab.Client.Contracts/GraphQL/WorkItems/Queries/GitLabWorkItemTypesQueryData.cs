using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Queries;

/// <summary>Data returned by a curated cursor-paginated namespace work-item-types query.</summary>
public sealed record GitLabWorkItemTypesQueryData
{
    /// <summary>The queried namespace, or null when it is missing or hidden from the caller.</summary>
    [JsonPropertyName("namespace")]
    public GitLabWorkItemNamespace? Namespace { get; init; }
}