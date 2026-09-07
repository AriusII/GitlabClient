using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Terraform state resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ITerraformStatesService), typeof(ITerraformStatesClient))]
internal interface ITerraformStatesRepository
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