using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for PlatformIntegrations, sitting between the public
///     <c>IPlatformIntegrationsClient</c> controller and <c>IPlatformIntegrationsRepository</c>'s raw
///     GitLab access. Mirrors the repository's method shapes 1:1 today (its implementation is
///     generated); this is the seam where request validation, caching, or cross-resource composition
///     would go once this resource needs more than pass-through.
/// </summary>
internal interface IPlatformIntegrationsService
{
    Task<GitLabJob> GetAllowedAgentsAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> GetIamUserInfoAsync(CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetGoogleCloudIntegrationSetupScriptAsync(ProjectId projectId,
        GoogleCloudIntegrationSetupScriptOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetGoogleCloudRunnerDeploymentSetupScriptAsync(ProjectId projectId,
        string googleCloudProjectId, CancellationToken cancellationToken = default);

    Task<GitLabTokenExchangeResult> ExchangeTokenAsync(TokenExchangeRequest request,
        CancellationToken cancellationToken = default);
}