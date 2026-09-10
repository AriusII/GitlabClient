using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class PushRulesClient(IGitLabApiConnection connection) : IPushRulesClient
{
    public Task<GitLabProjectPushRule> GetForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("push_rule").Build(),
            GitLabJsonContext.Default.GitLabProjectPushRule,
            cancellationToken);
    }

    public Task<GitLabProjectPushRule> CreateForProjectAsync(ProjectId projectId, CreatePushRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("push_rule").Build(),
            request,
            GitLabJsonContext.Default.CreatePushRuleRequest,
            GitLabJsonContext.Default.GitLabProjectPushRule,
            cancellationToken);
    }

    public Task<GitLabProjectPushRule> UpdateForProjectAsync(ProjectId projectId, UpdatePushRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("push_rule").Build(),
            request,
            GitLabJsonContext.Default.UpdatePushRuleRequest,
            GitLabJsonContext.Default.GitLabProjectPushRule,
            cancellationToken);
    }

    public Task DeleteForProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("push_rule").Build(),
            cancellationToken);
    }

    public Task<GitLabGroupPushRule> GetForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("push_rule").Build(),
            GitLabJsonContext.Default.GitLabGroupPushRule,
            cancellationToken);
    }

    public Task<GitLabGroupPushRule> CreateForGroupAsync(GroupId groupId, CreatePushRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("push_rule").Build(),
            request,
            GitLabJsonContext.Default.CreatePushRuleRequest,
            GitLabJsonContext.Default.GitLabGroupPushRule,
            cancellationToken);
    }

    public Task<GitLabGroupPushRule> UpdateForGroupAsync(GroupId groupId, UpdatePushRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("push_rule").Build(),
            request,
            GitLabJsonContext.Default.UpdatePushRuleRequest,
            GitLabJsonContext.Default.GitLabGroupPushRule,
            cancellationToken);
    }

    public Task DeleteForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("push_rule").Build(),
            cancellationToken);
    }
}