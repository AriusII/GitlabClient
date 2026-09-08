using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Search, sitting between the public <c>ISearchClient</c>
///     controller and <c>ISearchRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface ISearchService
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

    Task<JsonElement> ListSearchMigrationsAsync(CancellationToken cancellationToken = default);

    Task<GitLabSearchMigration> GetSearchMigrationAsync(string migrationId,
        CancellationToken cancellationToken = default);

    Task<GitLabSemanticCodeSearchResult> SearchProjectSemanticCodeAsync(ProjectId projectId, string q,
        SemanticCodeSearchOptions? options = null, CancellationToken cancellationToken = default);
}