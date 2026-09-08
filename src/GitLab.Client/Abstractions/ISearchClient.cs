using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Search" API area - the instance-wide <c>/search</c>, group-wide
///     <c>/groups/:id/search</c> and project-wide <c>/projects/:id/search</c> routes.
///     <para>
///         GitLab's <c>scope</c> query parameter is what decides the shape of the response, so there is
///         one method per (level, scope) pair rather than one method taking a scope. A scope-parameterised
///         method could not name a static <c>JsonTypeInfo&lt;T[]&gt;</c>, and resolving one at run time is
///         exactly the reflection this library is built to avoid.
///     </para>
///     <para>
///         Only the scopes whose result entity this library already models are exposed. The
///         <c>blobs</c>, <c>wiki_blobs</c> and <c>snippet_titles</c> scopes are absent because the spec
///         declares no response schema for them, and <c>groups</c>/<c>work_items</c> wait on the
///         corresponding resources.
///     </para>
///     <para>
///         Every call here is rate-limited by GitLab. The response headers are surfaced through
///         <c>IGitLabRateLimitTracker</c>; nothing retries automatically on 429, which
///         arrives as a <see cref="Exceptions.GitLabRateLimitExceededException" />.
///     </para>
/// </summary>
public interface ISearchClient
{
    /// <summary>Instance-wide project search (<c>GET /search?scope=projects</c>).</summary>
    IAsyncEnumerable<GitLabProject> SearchProjectsAsync(string search, SearchListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Instance-wide issue search (<c>GET /search?scope=issues</c>).</summary>
    IAsyncEnumerable<GitLabIssue> SearchIssuesAsync(string search, SearchListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Instance-wide merge-request search (<c>GET /search?scope=merge_requests</c>).</summary>
    IAsyncEnumerable<GitLabMergeRequest> SearchMergeRequestsAsync(string search, SearchListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Instance-wide user search (<c>GET /search?scope=users</c>).</summary>
    IAsyncEnumerable<GitLabUser> SearchUsersAsync(string search, SearchListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Project search within one group (<c>GET /groups/:id/search?scope=projects</c>).</summary>
    IAsyncEnumerable<GitLabProject> SearchGroupProjectsAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Issue search within one group (<c>GET /groups/:id/search?scope=issues</c>).</summary>
    IAsyncEnumerable<GitLabIssue> SearchGroupIssuesAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Merge-request search within one group (<c>GET /groups/:id/search?scope=merge_requests</c>).</summary>
    IAsyncEnumerable<GitLabMergeRequest> SearchGroupMergeRequestsAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Milestone search within one group (<c>GET /groups/:id/search?scope=milestones</c>).</summary>
    IAsyncEnumerable<GitLabMilestone> SearchGroupMilestonesAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Note (comment) search within one group (<c>GET /groups/:id/search?scope=notes</c>).</summary>
    IAsyncEnumerable<GitLabNote> SearchGroupNotesAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Commit-message search within one group (<c>GET /groups/:id/search?scope=commits</c>).</summary>
    IAsyncEnumerable<GitLabCommit> SearchGroupCommitsAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>User search within one group (<c>GET /groups/:id/search?scope=users</c>).</summary>
    IAsyncEnumerable<GitLabUser> SearchGroupUsersAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Issue search within one project (<c>GET /projects/:id/search?scope=issues</c>).</summary>
    IAsyncEnumerable<GitLabIssue> SearchProjectIssuesAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Merge-request search within one project (<c>GET /projects/:id/search?scope=merge_requests</c>).</summary>
    IAsyncEnumerable<GitLabMergeRequest> SearchProjectMergeRequestsAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Commit-message search within one project (<c>GET /projects/:id/search?scope=commits</c>).</summary>
    IAsyncEnumerable<GitLabCommit> SearchProjectCommitsAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Note (comment) search within one project (<c>GET /projects/:id/search?scope=notes</c>).</summary>
    IAsyncEnumerable<GitLabNote> SearchProjectNotesAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Milestone search within one project (<c>GET /projects/:id/search?scope=milestones</c>).</summary>
    IAsyncEnumerable<GitLabMilestone> SearchProjectMilestonesAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>User search within one project (<c>GET /projects/:id/search?scope=users</c>).</summary>
    IAsyncEnumerable<GitLabUser> SearchProjectUsersAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists every advanced search migration known to this instance (<c>GET /admin/search/migrations</c>).
    ///     Requires an administrator token. The spec declares no response schema for the list - unlike the
    ///     single-migration route below - so the answer is a raw <see cref="JsonElement" /> rather than an
    ///     invented array shape.
    /// </summary>
    Task<JsonElement> ListSearchMigrationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one advanced search migration by version or name
    ///     (<c>GET /admin/search/migrations/:migration_id</c>). Requires an administrator token.
    /// </summary>
    Task<GitLabSearchMigration> GetSearchMigrationAsync(string migrationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Searches a project's indexed code by meaning rather than keyword
    ///     (<c>GET /projects/:id/search/semantic</c>). Requires semantic code search to be enabled and
    ///     indexed for the project's namespace. Introduced in GitLab 18.11.
    /// </summary>
    Task<GitLabSemanticCodeSearchResult> SearchProjectSemanticCodeAsync(ProjectId projectId, string q,
        SemanticCodeSearchOptions? options = null, CancellationToken cancellationToken = default);
}