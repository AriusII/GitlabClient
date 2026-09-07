using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Freeze periods, sitting between the public
///     <c>IFreezePeriodsClient</c> controller and <c>IFreezePeriodsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IFreezePeriodsService
{
    IAsyncEnumerable<GitLabFreezePeriod> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabFreezePeriod> GetAsync(ProjectId projectId, long freezePeriodId,
        CancellationToken cancellationToken = default);

    Task<GitLabFreezePeriod> CreateAsync(ProjectId projectId, CreateFreezePeriodRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabFreezePeriod> UpdateAsync(ProjectId projectId, long freezePeriodId,
        UpdateFreezePeriodRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long freezePeriodId, CancellationToken cancellationToken = default);
}