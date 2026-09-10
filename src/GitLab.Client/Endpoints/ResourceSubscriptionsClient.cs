using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Responses;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ResourceSubscriptionsClient(IGitLabApiConnection connection)
    : IResourceSubscriptionsClient
{
    private const string SubscribeAction = "subscribe";

    private const string UnsubscribeAction = "unsubscribe";

    public Task<GitLabIssue> SubscribeToIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            IssueRoute(projectId, issueIid, SubscribeAction),
            GitLabJsonContext.Default.GitLabIssue,
            cancellationToken);
    }

    public Task<GitLabIssue> UnsubscribeFromIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            IssueRoute(projectId, issueIid, UnsubscribeAction),
            GitLabJsonContext.Default.GitLabIssue,
            cancellationToken);
    }

    public Task<GitLabMergeRequest> SubscribeToMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestRoute(projectId, mergeRequestIid, SubscribeAction),
            GitLabJsonContext.Default.GitLabMergeRequest,
            cancellationToken);
    }

    public Task<GitLabMergeRequest> UnsubscribeFromMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestRoute(projectId, mergeRequestIid, UnsubscribeAction),
            GitLabJsonContext.Default.GitLabMergeRequest,
            cancellationToken);
    }

    public Task<GitLabLabel> SubscribeToProjectLabelAsync(ProjectId projectId, string labelIdOrName,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectLabelRoute(projectId, labelIdOrName, SubscribeAction),
            GitLabJsonContext.Default.GitLabLabel,
            cancellationToken);
    }

    public Task<GitLabLabel> UnsubscribeFromProjectLabelAsync(ProjectId projectId, string labelIdOrName,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectLabelRoute(projectId, labelIdOrName, UnsubscribeAction),
            GitLabJsonContext.Default.GitLabLabel,
            cancellationToken);
    }

    public Task<GitLabGroupLabel> SubscribeToGroupLabelAsync(GroupId groupId, string labelIdOrName,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GroupLabelRoute(groupId, labelIdOrName, SubscribeAction),
            GitLabJsonContext.Default.GitLabGroupLabel,
            cancellationToken);
    }

    public Task<GitLabGroupLabel> UnsubscribeFromGroupLabelAsync(GroupId groupId, string labelIdOrName,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GroupLabelRoute(groupId, labelIdOrName, UnsubscribeAction),
            GitLabJsonContext.Default.GitLabGroupLabel,
            cancellationToken);
    }

    // Every operation in this tag is the same route with "subscribe" or "unsubscribe" as its last word, so
    // the four subscribable roots are built once each and the action is passed in - the alternative is
    // eight near-identical chains differing by a single literal.
    private static Uri IssueRoute(ProjectId projectId, long issueIid, string action)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("issues")
            .Segment(issueIid)
            .Literal(action)
            .Build();
    }

    private static Uri MergeRequestRoute(ProjectId projectId, long mergeRequestIid, string action)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("merge_requests")
            .Segment(mergeRequestIid)
            .Literal(action)
            .Build();
    }

    private static Uri ProjectLabelRoute(ProjectId projectId, string labelIdOrName, string action)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("labels")
            .Escaped(labelIdOrName)
            .Literal(action)
            .Build();
    }

    private static Uri GroupLabelRoute(GroupId groupId, string labelIdOrName, string action)
    {
        return GitLabRouteBuilder.Create("groups")
            .Segment(groupId)
            .Literal("labels")
            .Escaped(labelIdOrName)
            .Literal(action)
            .Build();
    }
}