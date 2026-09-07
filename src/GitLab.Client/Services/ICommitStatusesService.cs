using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for CommitStatuses, sitting between the public
///     <c>ICommitStatusesClient</c> controller and <c>ICommitStatusesRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the seam
///     where request validation, caching, or cross-resource composition would go once the resource needs
///     more than pass-through.
/// </summary>
internal interface ICommitStatusesService
{
    IAsyncEnumerable<GitLabCommitStatus> ListAsync(ProjectId projectId, string sha,
        CommitStatusListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabCommitStatus> CreateAsync(ProjectId projectId, string sha, CreateCommitStatusRequest request,
        CancellationToken cancellationToken = default);
}