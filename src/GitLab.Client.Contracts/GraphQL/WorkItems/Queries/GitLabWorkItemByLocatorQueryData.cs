using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Queries;

/// <summary>Data returned by a curated namespace/IID work-item query.</summary>
public sealed record GitLabWorkItemByLocatorQueryData
{
    /// <summary>
    ///     The queried namespace, or null when it is missing or not visible. Its
    ///     <see cref="GitLabWorkItemNamespace.WorkItem" />
    ///     member is null when the IID is absent or hidden.
    /// </summary>
    [JsonPropertyName("namespace")]
    public GitLabWorkItemNamespace? Namespace { get; init; }
}