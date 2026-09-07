using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Terraform state, sitting between the public
///     <c>ITerraformStatesClient</c> controller and <c>ITerraformStatesRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the seam
///     where request validation, caching, or cross-resource composition would go once the resource needs
///     more than pass-through.
/// </summary>
internal interface ITerraformStatesService
{
    Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, string name, string? lockId = null,
        CancellationToken cancellationToken = default);

    Task UploadAsync(ProjectId projectId, string name, GitLabFileUpload state,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    Task AuthorizeUploadAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    Task LockAsync(ProjectId projectId, string name, LockTerraformStateRequest request,
        CancellationToken cancellationToken = default);

    Task UnlockAsync(ProjectId projectId, string name, string? lockId = null,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadVersionAsync(ProjectId projectId, string name, long serial,
        CancellationToken cancellationToken = default);

    Task DeleteVersionAsync(ProjectId projectId, string name, long serial,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabTerraformStateProtectionRule> ListProtectionRulesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabTerraformStateProtectionRule> CreateProtectionRuleAsync(ProjectId projectId,
        CreateTerraformStateProtectionRuleRequest request, CancellationToken cancellationToken = default);

    Task<GitLabTerraformStateProtectionRule> UpdateProtectionRuleAsync(ProjectId projectId, long ruleId,
        UpdateTerraformStateProtectionRuleRequest request, CancellationToken cancellationToken = default);

    Task DeleteProtectionRuleAsync(ProjectId projectId, long ruleId, CancellationToken cancellationToken = default);
}