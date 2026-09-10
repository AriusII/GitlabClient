using GitLab.Client.GraphQL.Protocol;
using GitLab.Client.GraphQL.WorkItems;
using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Mutations;
using GitLab.Client.GraphQL.WorkItems.Queries;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Curated GitLab GraphQL Work Item operations, including namespace-local lookup, cursor pagination, and the
///     create, update, and delete mutations supported by this SDK.
/// </summary>
/// <remarks>
///     Work Item widgets are configurable by work-item type, GitLab tier, and authorization. The returned envelope
///     therefore preserves GraphQL-level errors alongside partial data instead of inferring REST-style success from
///     the HTTP status alone. Use <see cref="IGraphQLClient" />'s generic document executor with caller-owned
///     source-generated metadata for schema operations not represented by this deliberately bounded surface.
///     GitLab's GraphQL schema is versionless, and Work Item fields or widgets can be experimental, so this interface
///     defines the SDK's curated operation contracts without asserting server-side schema stability.
/// </remarks>
public interface IGraphQLWorkItemsClient
{
    /// <summary>Gets one cursor page of the work-item types available to a project or group namespace.</summary>
    /// <param name="namespacePath">The project or group full path.</param>
    /// <param name="first">The positive maximum number of types to return, or null for GitLab's default.</param>
    /// <param name="after">The opaque cursor after which to continue, or null for the first page.</param>
    /// <param name="cancellationToken">Cancels the HTTP operation.</param>
    /// <returns>The GraphQL envelope containing the selected namespace and type page.</returns>
    Task<GitLabGraphQLResponse<GitLabWorkItemTypesQueryData>> GetTypesAsync(
        string namespacePath,
        int? first = null,
        string? after = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one work item from its opaque GraphQL global ID.</summary>
    /// <param name="id">The work item's opaque GraphQL global ID.</param>
    /// <param name="cancellationToken">Cancels the HTTP operation.</param>
    /// <returns>The GraphQL envelope containing the work item when it is visible to the caller.</returns>
    Task<GitLabGraphQLResponse<GitLabWorkItemByIdQueryData>> GetAsync(
        GitLabGraphQLGlobalId id,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one work item from its opaque GraphQL global ID with an explicit closed widget profile.</summary>
    /// <param name="id">The work item's opaque GraphQL global ID.</param>
    /// <param name="widgetProfile">The closed selection budget for optional work-item widgets.</param>
    /// <param name="cancellationToken">Cancels the HTTP operation.</param>
    /// <returns>The GraphQL envelope containing the requested core and widget projections when visible.</returns>
    Task<GitLabGraphQLResponse<GitLabWorkItemByIdQueryData>> GetAsync(
        GitLabGraphQLGlobalId id,
        GitLabWorkItemWidgetProfile widgetProfile,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one work item from its namespace path and namespace-local IID.</summary>
    /// <param name="locator">The validated project/group namespace and positive work-item IID.</param>
    /// <param name="cancellationToken">Cancels the HTTP operation.</param>
    /// <returns>The GraphQL envelope containing the namespace and work item when they are visible to the caller.</returns>
    Task<GitLabGraphQLResponse<GitLabWorkItemByLocatorQueryData>> GetAsync(
        GitLabWorkItemLocator locator,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one work item from its namespace path and IID with an explicit closed widget profile.</summary>
    /// <param name="locator">The validated project/group namespace and positive work-item IID.</param>
    /// <param name="widgetProfile">The closed selection budget for optional work-item widgets.</param>
    /// <param name="cancellationToken">Cancels the HTTP operation.</param>
    /// <returns>The GraphQL envelope containing the requested core and widget projections when visible.</returns>
    Task<GitLabGraphQLResponse<GitLabWorkItemByLocatorQueryData>> GetAsync(
        GitLabWorkItemLocator locator,
        GitLabWorkItemWidgetProfile widgetProfile,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one cursor page of work items belonging directly to a project or group namespace.</summary>
    /// <param name="namespacePath">The project or group full path.</param>
    /// <param name="first">The positive maximum number of work items to return, or null for GitLab's default.</param>
    /// <param name="after">The opaque cursor after which to continue, or null for the first page.</param>
    /// <param name="cancellationToken">Cancels the HTTP operation.</param>
    /// <returns>The GraphQL envelope containing the selected namespace and work-item page.</returns>
    Task<GitLabGraphQLResponse<GitLabWorkItemsQueryData>> ListAsync(
        string namespacePath,
        int? first = null,
        string? after = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one cursor page of work items with an explicit closed widget profile.</summary>
    /// <param name="namespacePath">The project or group full path.</param>
    /// <param name="first">The positive maximum number of work items to return, or null for GitLab's default.</param>
    /// <param name="after">The opaque cursor after which to continue, or null for the first page.</param>
    /// <param name="widgetProfile">The closed selection budget for optional work-item widgets.</param>
    /// <param name="cancellationToken">Cancels the HTTP operation.</param>
    /// <returns>The GraphQL envelope containing the requested core and widget projections for this page.</returns>
    Task<GitLabGraphQLResponse<GitLabWorkItemsQueryData>> ListAsync(
        string namespacePath,
        int? first,
        string? after,
        GitLabWorkItemWidgetProfile widgetProfile,
        CancellationToken cancellationToken = default);

    /// <summary>Creates one work item from the supplied namespace-specific work-item type and widget inputs.</summary>
    /// <param name="input">The validated work-item create input.</param>
    /// <param name="cancellationToken">Cancels the HTTP operation.</param>
    /// <returns>The GraphQL envelope, including mutation payload errors reported by GitLab.</returns>
    Task<GitLabGraphQLResponse<GitLabWorkItemCreateMutationData>> CreateAsync(
        GitLabWorkItemCreateInput input,
        CancellationToken cancellationToken = default);

    /// <summary>Applies the supplied partial update to one work item.</summary>
    /// <param name="input">The validated work-item update input.</param>
    /// <param name="cancellationToken">Cancels the HTTP operation.</param>
    /// <returns>The GraphQL envelope, including mutation payload errors reported by GitLab.</returns>
    Task<GitLabGraphQLResponse<GitLabWorkItemUpdateMutationData>> UpdateAsync(
        GitLabWorkItemUpdateInput input,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one work item identified by its opaque GraphQL global ID.</summary>
    /// <param name="input">The validated work-item delete input.</param>
    /// <param name="cancellationToken">Cancels the HTTP operation.</param>
    /// <returns>The GraphQL envelope, including mutation payload errors reported by GitLab.</returns>
    Task<GitLabGraphQLResponse<GitLabWorkItemDeleteMutationData>> DeleteAsync(
        GitLabWorkItemDeleteInput input,
        CancellationToken cancellationToken = default);
}