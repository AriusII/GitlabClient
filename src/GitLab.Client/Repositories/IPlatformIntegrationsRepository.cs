using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the PlatformIntegrations resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         Four unrelated OpenAPI tags share this one resource - Agents, Authn, Token exchange and
///         Project Google Cloud integration - because each contributes only one or two operations, none
///         individually worth a four-file chain. See <see cref="IPlatformIntegrationsClient" /> for why.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IPlatformIntegrationsService), typeof(IPlatformIntegrationsClient))]
internal interface IPlatformIntegrationsRepository
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