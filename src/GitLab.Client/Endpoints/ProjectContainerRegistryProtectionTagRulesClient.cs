using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ProjectContainerRegistryProtectionTagRulesClient(IGitLabApiConnection connection)
    : IProjectContainerRegistryProtectionTagRulesClient
{
    public IAsyncEnumerable<GitLabContainerRegistryProtectionTagRule> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            RulesRoute(projectId).Build(),
            GitLabJsonContext.Default.GitLabContainerRegistryProtectionTagRuleArray,
            cancellationToken);
    }

    public Task<GitLabContainerRegistryProtectionTagRule> CreateAsync(ProjectId projectId,
        CreateContainerRegistryProtectionTagRuleRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RulesRoute(projectId).Build(),
            request,
            GitLabJsonContext.Default.CreateContainerRegistryProtectionTagRuleRequest,
            GitLabJsonContext.Default.GitLabContainerRegistryProtectionTagRule,
            cancellationToken);
    }

    public Task<GitLabContainerRegistryProtectionTagRule> UpdateAsync(ProjectId projectId, long ruleId,
        UpdateContainerRegistryProtectionTagRuleRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            RulesRoute(projectId).Segment(ruleId).Build(),
            request,
            GitLabJsonContext.Default.UpdateContainerRegistryProtectionTagRuleRequest,
            GitLabJsonContext.Default.GitLabContainerRegistryProtectionTagRule,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long ruleId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(RulesRoute(projectId).Segment(ruleId).Build(), cancellationToken);
    }

    private static GitLabRouteBuilder RulesRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("registry").Literal("protection")
            .Literal("tag").Literal("rules");
    }
}