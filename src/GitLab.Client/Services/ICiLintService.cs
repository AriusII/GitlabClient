using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for CI Lint, sitting between the public <c>ICiLintClient</c>
///     controller and <c>ICiLintRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface ICiLintService
{
    Task<GitLabCiLintResult> ValidateProjectConfigurationAsync(ProjectId projectId, CiLintOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabCiLintResult> ValidateAsync(ProjectId projectId, ValidateCiConfigurationRequest request,
        CancellationToken cancellationToken = default);
}