using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Models;
using GitLab.Client.Models;

namespace GitLab.Client.Composition.WorkItems;

/// <summary>
///     A rich, client-side work-item aggregate assembled from selected REST and GraphQL route DTOs.
/// </summary>
/// <remarks>
///     This is a pure composition contract, not a cache or a new API resource. It preserves the original DTOs so
///     callers retain every endpoint-specific field, and adds a normalized view for the supported GraphQL widgets.
///     Consult <see cref="Sources" /> before interpreting a null property: it can mean a source was intentionally
///     omitted, unavailable, or returned no visible resource.
/// </remarks>
public sealed record GitLabWorkItemComposition
{
    /// <summary>The plan that selected the composition's source slices.</summary>
    public required GitLabWorkItemCompositionLoadPlan LoadPlan { get; init; }

    /// <summary>The locator supplied by the caller, when available.</summary>
    public GitLabWorkItemLocator? Locator { get; init; }

    /// <summary>The selected REST project DTO, or null when its source was not selected or unavailable.</summary>
    public GitLabProject? RestProject { get; init; }

    /// <summary>The selected REST issue DTO, or null when its source was not selected or unavailable.</summary>
    public GitLabIssue? RestIssue { get; init; }

    /// <summary>The selected legacy REST epic DTO, or null when its source was not selected or unavailable.</summary>
    public GitLabEpic? LegacyRestEpic { get; init; }

    /// <summary>The selected GraphQL work-item DTO, or null when its source was not selected or unavailable.</summary>
    public GitLabWorkItem? GraphQLWorkItem { get; init; }

    /// <summary>The normalized values of the selected curated GraphQL widgets.</summary>
    public required GitLabWorkItemWidgetComposition GraphQLWidgets { get; init; }

    /// <summary>The requested-versus-available state of every supported source slice.</summary>
    public required GitLabWorkItemCompositionSources Sources { get; init; }

    /// <summary>Safe cross-source namespace, IID, and REST-project-ID diagnostics.</summary>
    public required GitLabWorkItemCompositionCorrelation Correlation { get; init; }

    /// <summary>Returns whether the plan requested every flag in <paramref name="section" />.</summary>
    public bool IsRequested(GitLabWorkItemCompositionSection section)
    {
        return LoadPlan.Includes(section);
    }
}