using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class SearchRepository(IGitLabApiConnection connection) : ISearchRepository
{
    private const string ProjectsScope = "projects";

    private const string IssuesScope = "issues";

    private const string MergeRequestsScope = "merge_requests";

    private const string UsersScope = "users";

    private const string CommitsScope = "commits";

    private const string NotesScope = "notes";

    private const string MilestonesScope = "milestones";

    public IAsyncEnumerable<GitLabProject> SearchProjectsAsync(string search, SearchListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return SearchInstanceAsync(search, ProjectsScope, options, GitLabJsonContext.Default.GitLabProjectArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabIssue> SearchIssuesAsync(string search, SearchListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return SearchInstanceAsync(search, IssuesScope, options, GitLabJsonContext.Default.GitLabIssueArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequest> SearchMergeRequestsAsync(string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SearchInstanceAsync(search, MergeRequestsScope, options,
            GitLabJsonContext.Default.GitLabMergeRequestArray, cancellationToken);
    }

    public IAsyncEnumerable<GitLabUser> SearchUsersAsync(string search, SearchListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return SearchInstanceAsync(search, UsersScope, options, GitLabJsonContext.Default.GitLabUserArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProject> SearchGroupProjectsAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SearchGroupAsync(groupId, search, ProjectsScope, options, GitLabJsonContext.Default.GitLabProjectArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabIssue> SearchGroupIssuesAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SearchGroupAsync(groupId, search, IssuesScope, options, GitLabJsonContext.Default.GitLabIssueArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequest> SearchGroupMergeRequestsAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SearchGroupAsync(groupId, search, MergeRequestsScope, options,
            GitLabJsonContext.Default.GitLabMergeRequestArray, cancellationToken);
    }

    public IAsyncEnumerable<GitLabMilestone> SearchGroupMilestonesAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SearchGroupAsync(groupId, search, MilestonesScope, options,
            GitLabJsonContext.Default.GitLabMilestoneArray, cancellationToken);
    }

    public IAsyncEnumerable<GitLabNote> SearchGroupNotesAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SearchGroupAsync(groupId, search, NotesScope, options, GitLabJsonContext.Default.GitLabNoteArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabCommit> SearchGroupCommitsAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SearchGroupAsync(groupId, search, CommitsScope, options, GitLabJsonContext.Default.GitLabCommitArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabUser> SearchGroupUsersAsync(GroupId groupId, string search,
        SearchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SearchGroupAsync(groupId, search, UsersScope, options, GitLabJsonContext.Default.GitLabUserArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabIssue> SearchProjectIssuesAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SearchProjectAsync(projectId, search, IssuesScope, options, GitLabJsonContext.Default.GitLabIssueArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequest> SearchProjectMergeRequestsAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SearchProjectAsync(projectId, search, MergeRequestsScope, options,
            GitLabJsonContext.Default.GitLabMergeRequestArray, cancellationToken);
    }

    public IAsyncEnumerable<GitLabCommit> SearchProjectCommitsAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SearchProjectAsync(projectId, search, CommitsScope, options,
            GitLabJsonContext.Default.GitLabCommitArray, cancellationToken);
    }

    public IAsyncEnumerable<GitLabNote> SearchProjectNotesAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SearchProjectAsync(projectId, search, NotesScope, options, GitLabJsonContext.Default.GitLabNoteArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMilestone> SearchProjectMilestonesAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SearchProjectAsync(projectId, search, MilestonesScope, options,
            GitLabJsonContext.Default.GitLabMilestoneArray, cancellationToken);
    }

    public IAsyncEnumerable<GitLabUser> SearchProjectUsersAsync(ProjectId projectId, string search,
        ProjectSearchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return SearchProjectAsync(projectId, search, UsersScope, options, GitLabJsonContext.Default.GitLabUserArray,
            cancellationToken);
    }

    /// <summary>
    ///     The spec declares no response schema for this list (unlike the single-migration route), so the
    ///     answer is a raw <see cref="JsonElement" /> rather than an invented array shape.
    /// </summary>
    public Task<JsonElement> ListSearchMigrationsAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("admin")
                .Literal("search")
                .Literal("migrations")
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<GitLabSearchMigration> GetSearchMigrationAsync(string migrationId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("admin")
                .Literal("search")
                .Literal("migrations")
                .Escaped(migrationId)
                .Build(),
            GitLabJsonContext.Default.GitLabSearchMigration,
            cancellationToken);
    }

    /// <summary>
    ///     A single grouped-by-file result object, not a page of items - <c>GetAsync</c> rather than
    ///     <c>GetPagedAsync</c>.
    /// </summary>
    public Task<GitLabSemanticCodeSearchResult> SearchProjectSemanticCodeAsync(ProjectId projectId, string q,
        SemanticCodeSearchOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("search")
                .Literal("semantic")
                .Query("q", q)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabSemanticCodeSearchResult,
            cancellationToken);
    }

    private IAsyncEnumerable<TItem> SearchInstanceAsync<TItem>(string search, string scope, SearchListOptions? options,
        JsonTypeInfo<TItem[]> pageTypeInfo, CancellationToken cancellationToken)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("search")
                .Query("search", search)
                .Query("scope", scope)
                .QueryFrom(options)
                .Build(),
            pageTypeInfo,
            cancellationToken);
    }

    private IAsyncEnumerable<TItem> SearchGroupAsync<TItem>(GroupId groupId, string search, string scope,
        SearchListOptions? options, JsonTypeInfo<TItem[]> pageTypeInfo, CancellationToken cancellationToken)
    {
        // The vendored spec spells this path "/groups/{id}/(-/)search". The "(-/)" is a Grape
        // optional-segment artifact that leaks into the generated OpenAPI document, not a literal path
        // segment - the real route is "groups/{id}/search".
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("search")
                .Query("search", search)
                .Query("scope", scope)
                .QueryFrom(options)
                .Build(),
            pageTypeInfo,
            cancellationToken);
    }

    private IAsyncEnumerable<TItem> SearchProjectAsync<TItem>(ProjectId projectId, string search, string scope,
        ProjectSearchListOptions? options, JsonTypeInfo<TItem[]> pageTypeInfo, CancellationToken cancellationToken)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("search")
                .Query("search", search)
                .Query("scope", scope)
                .QueryFrom(options)
                .Build(),
            pageTypeInfo,
            cancellationToken);
    }
}