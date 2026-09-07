using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Jira Connect / GitLab for Jira (Forge) resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IJiraConnectService), typeof(IJiraConnectClient))]
internal interface IJiraConnectRepository
{
    Task<GitLabJiraConnectResult> SubscribeNamespaceAsync(SubscribeJiraConnectNamespaceRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabJiraConnectSubscription> ListForgeSubscriptionsAsync(
        CancellationToken cancellationToken = default);

    Task<GitLabJiraConnectResult> CreateForgeSubscriptionAsync(CreateJiraForgeSubscriptionRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabJiraConnectResult> DeleteForgeSubscriptionAsync(long subscriptionId,
        CancellationToken cancellationToken = default);

    Task<GitLabJiraConnectResult> UpdateForgeInstallationAsync(UpdateJiraForgeInstallationRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabJiraConnectResult> RegisterForgeTokenAsync(CancellationToken cancellationToken = default);
}