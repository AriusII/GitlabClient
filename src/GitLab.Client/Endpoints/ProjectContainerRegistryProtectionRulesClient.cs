using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ProjectContainerRegistryProtectionRulesClient(IGitLabApiConnection connection)
    : IProjectContainerRegistryProtectionRulesClient
{
    public IAsyncEnumerable<GitLabContainerRegistryProtectionRule> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            RulesRoute(projectId).Build(),
            GitLabJsonContext.Default.GitLabContainerRegistryProtectionRuleArray,
            cancellationToken);
    }

    public Task<GitLabContainerRegistryProtectionRule> CreateAsync(ProjectId projectId,
        CreateContainerRegistryProtectionRuleRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RulesRoute(projectId).Build(),
            request,
            GitLabJsonContext.Default.CreateContainerRegistryProtectionRuleRequest,
            GitLabJsonContext.Default.GitLabContainerRegistryProtectionRule,
            cancellationToken);
    }

    public Task<GitLabContainerRegistryProtectionRule> UpdateAsync(ProjectId projectId, long ruleId,
        UpdateContainerRegistryProtectionRuleRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            RulesRoute(projectId).Segment(ruleId).Build(),
            request,
            GitLabJsonContext.Default.UpdateContainerRegistryProtectionRuleRequest,
            GitLabJsonContext.Default.GitLabContainerRegistryProtectionRule,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long ruleId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(RulesRoute(projectId).Segment(ruleId).Build(), cancellationToken);
    }

    private static GitLabRouteBuilder RulesRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("registry").Literal("protection")
            .Literal("repository").Literal("rules");
    }
}