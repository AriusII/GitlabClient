namespace GitLab.Client.Composition.WorkItems;

/// <summary>
///     Non-authoritative correlation diagnostics produced while assembling a work-item composition.
/// </summary>
/// <remarks>
///     These diagnostics compare only like-for-like namespace paths, IIDs, and REST project database IDs. They do
///     not make a relationship authoritative: GitLab authorization can return reduced projections and a work-item
///     IID is local to its namespace. There is deliberately no comparison between <c>GitLabEpic.Id</c> (or
///     <c>GitLabEpic.WorkItemId</c>) and a GraphQL global ID: GraphQL IDs are opaque strings, not numeric REST IDs.
/// </remarks>
public sealed record GitLabWorkItemCompositionCorrelation
{
    /// <summary>The comparison of the requested locator namespace and REST project path-with-namespace.</summary>
    public required GitLabWorkItemCompositionCorrelationStatus LocatorNamespaceToRestProjectPath { get; init; }

    /// <summary>The comparison of the requested locator IID and the REST issue IID.</summary>
    public required GitLabWorkItemCompositionCorrelationStatus LocatorIidToRestIssueIid { get; init; }

    /// <summary>The comparison of the requested locator IID and the legacy REST epic IID.</summary>
    public required GitLabWorkItemCompositionCorrelationStatus LocatorIidToLegacyRestEpicIid { get; init; }

    /// <summary>The comparison of the requested locator IID and the GraphQL work-item IID.</summary>
    public required GitLabWorkItemCompositionCorrelationStatus LocatorIidToGraphQLWorkItemIid { get; init; }

    /// <summary>The comparison of the REST issue project ID and REST project ID.</summary>
    public required GitLabWorkItemCompositionCorrelationStatus RestIssueProjectIdToRestProjectId { get; init; }

    /// <summary>The comparison of the REST issue IID and GraphQL work-item IID.</summary>
    public required GitLabWorkItemCompositionCorrelationStatus RestIssueIidToGraphQLWorkItemIid { get; init; }

    /// <summary>The comparison of the legacy REST epic IID and GraphQL work-item IID.</summary>
    public required GitLabWorkItemCompositionCorrelationStatus LegacyRestEpicIidToGraphQLWorkItemIid { get; init; }
}