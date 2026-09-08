using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Search resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ISearchService), typeof(ISearchClient))]
internal interface ISearchRepository
{
    IAsyncEnumerable<GitLabProject> SearchProjectsAsync(string search, SearchListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabIssue> SearchIssuesAsync(string search, SearchListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequest> SearchMergeRequestsAsync(string search, SearchListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabUser> SearchUsersAsync(string search, SearchListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProject> SearchGroupProjectsAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabIssue> SearchGroupIssuesAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequest> SearchGroupMergeRequestsAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMilestone> SearchGroupMilestonesAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNote> SearchGroupNotesAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabCommit> SearchGroupCommitsAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabUser> SearchGroupUsersAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabIssue> SearchProjectIssuesAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequest> SearchProjectMergeRequestsAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabCommit> SearchProjectCommitsAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNote> SearchProjectNotesAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMilestone> SearchProjectMilestonesAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabUser> SearchProjectUsersAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists every advanced search migration known to this instance. The spec declares no response
    ///     schema for the list, unlike the single-migration route, so the answer is a raw
    ///     <see cref="JsonElement" /> rather than an invented array shape.
    /// </summary>
    Task<JsonElement> ListSearchMigrationsAsync(CancellationToken cancellationToken = default);

    Task<GitLabSearchMigration> GetSearchMigrationAsync(string migrationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     A single grouped-by-file result object, not a page of items - <c>GetAsync</c> rather than
    ///     <c>GetPagedAsync</c>.
    /// </summary>
    Task<GitLabSemanticCodeSearchResult> SearchProjectSemanticCodeAsync(ProjectId projectId, string q,
        SemanticCodeSearchOptions? options = null, CancellationToken cancellationToken = default);
}