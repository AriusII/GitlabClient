using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class JiraConnectClient(IGitLabApiConnection connection) : IJiraConnectClient
{
    public Task<GitLabJiraConnectResult> SubscribeNamespaceAsync(SubscribeJiraConnectNamespaceRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("integrations").Literal("jira_connect").Literal("subscriptions").Build(),
            request,
            GitLabJsonContext.Default.SubscribeJiraConnectNamespaceRequest,
            GitLabJsonContext.Default.GitLabJiraConnectResult,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabJiraConnectSubscription> ListForgeSubscriptionsAsync(
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("integrations").Literal("jira_forge").Literal("subscriptions").Build(),
            GitLabJsonContext.Default.GitLabJiraConnectSubscriptionArray,
            cancellationToken);
    }

    public Task<GitLabJiraConnectResult> CreateForgeSubscriptionAsync(CreateJiraForgeSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("integrations").Literal("jira_forge").Literal("subscriptions").Build(),
            request,
            GitLabJsonContext.Default.CreateJiraForgeSubscriptionRequest,
            GitLabJsonContext.Default.GitLabJiraConnectResult,
            cancellationToken);
    }

    public Task<GitLabJiraConnectResult> DeleteForgeSubscriptionAsync(long subscriptionId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("integrations").Literal("jira_forge").Literal("subscriptions")
                .Segment(subscriptionId).Build(),
            GitLabJsonContext.Default.GitLabJiraConnectResult,
            cancellationToken);
    }

    public Task<GitLabJiraConnectResult> UpdateForgeInstallationAsync(UpdateJiraForgeInstallationRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("integrations").Literal("jira_forge").Literal("installation").Build(),
            request,
            GitLabJsonContext.Default.UpdateJiraForgeInstallationRequest,
            GitLabJsonContext.Default.GitLabJiraConnectResult,
            cancellationToken);
    }

    public Task<GitLabJiraConnectResult> RegisterForgeTokenAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("integrations").Literal("jira_forge").Literal("installation")
                .Literal("forge_token").Build(),
            GitLabJsonContext.Default.GitLabJiraConnectResult,
            cancellationToken);
    }
}