using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Jira Connect / GitLab for Jira (Forge), sitting between the
///     public <c>IJiraConnectClient</c> controller and <c>IJiraConnectRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IJiraConnectService
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