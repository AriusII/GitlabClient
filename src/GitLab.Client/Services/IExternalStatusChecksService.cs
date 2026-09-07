using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for External status checks, sitting between the public
///     <c>IExternalStatusChecksClient</c> controller and <c>IExternalStatusChecksRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource needs
///     more than pass-through.
/// </summary>
internal interface IExternalStatusChecksService
{
    IAsyncEnumerable<GitLabExternalStatusCheck> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabExternalStatusCheck> CreateAsync(ProjectId projectId, CreateExternalStatusCheckRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabExternalStatusCheck> UpdateAsync(ProjectId projectId, long checkId,
        UpdateExternalStatusCheckRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long checkId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequestStatusCheck> ListForMergeRequestAsync(ProjectId projectId,
        long mergeRequestIid, CancellationToken cancellationToken = default);

    Task<GitLabStatusCheckResponse> SetStatusAsync(ProjectId projectId, long mergeRequestIid,
        SetStatusCheckStatusRequest request, CancellationToken cancellationToken = default);

    Task RetryAsync(ProjectId projectId, long mergeRequestIid, long externalStatusCheckId,
        CancellationToken cancellationToken = default);
}