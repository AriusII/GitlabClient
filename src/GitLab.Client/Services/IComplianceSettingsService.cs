using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for compliance policy settings, sitting between the public
///     <c>IComplianceSettingsClient</c> controller and <c>IComplianceSettingsRepository</c>'s raw
///     GitLab access. Mirrors the repository's method shapes 1:1 today (its implementation is
///     generated); this is the seam where request validation, caching, or cross-resource composition
///     would go once the resource needs more than pass-through.
/// </summary>
internal interface IComplianceSettingsService
{
    Task<GitLabCompliancePolicySettings> GetAsync(CancellationToken cancellationToken = default);

    Task<GitLabCompliancePolicySettings> UpdateAsync(UpdateCompliancePolicySettingsRequest request,
        CancellationToken cancellationToken = default);

    Task SetExternalControlStatusAsync(ProjectId projectId, long controlId,
        SetComplianceExternalControlStatusRequest request, CancellationToken cancellationToken = default);
}