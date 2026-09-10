using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Models;
using GitLab.Client.Models;

namespace GitLab.Client.Composition.WorkItems;

/// <summary>
///     Existing REST and GraphQL route DTOs supplied to <see cref="GitLabWorkItemCompositionMapper" />.
/// </summary>
/// <remarks>
///     A null source means that its request was not made, failed before producing a payload, returned no visible
///     item, or was intentionally omitted. <see cref="GitLabWorkItemComposition.Sources" /> keeps that availability
///     distinct from the caller's requested <see cref="GitLabWorkItemCompositionLoadPlan" />.
/// </remarks>
public sealed record GitLabWorkItemCompositionInput
{
    /// <summary>
    ///     The namespace/IID locator used to load the item, when the caller has one. It is used only for explicit
    ///     namespace and IID diagnostics; it is never converted to a GraphQL global ID.
    /// </summary>
    public GitLabWorkItemLocator? Locator { get; init; }

    /// <summary>A REST <c>GET /projects/:id</c> result.</summary>
    public GitLabProject? RestProject { get; init; }

    /// <summary>A REST issue result.</summary>
    public GitLabIssue? RestIssue { get; init; }

    /// <summary>A legacy REST epic result.</summary>
    public GitLabEpic? LegacyRestEpic { get; init; }

    /// <summary>A GraphQL work-item result with any selected curated widget fragments.</summary>
    public GitLabWorkItem? GraphQLWorkItem { get; init; }
}