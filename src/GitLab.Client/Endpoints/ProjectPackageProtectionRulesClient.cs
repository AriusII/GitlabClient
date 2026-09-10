using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ProjectPackageProtectionRulesClient(IGitLabApiConnection connection)
    : IProjectPackageProtectionRulesClient
{
    public IAsyncEnumerable<GitLabPackageProtectionRule> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            RulesRoute(projectId).Build(),
            GitLabJsonContext.Default.GitLabPackageProtectionRuleArray,
            cancellationToken);
    }

    public Task<GitLabPackageProtectionRule> CreateAsync(ProjectId projectId,
        CreatePackageProtectionRuleRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RulesRoute(projectId).Build(),
            request,
            GitLabJsonContext.Default.CreatePackageProtectionRuleRequest,
            GitLabJsonContext.Default.GitLabPackageProtectionRule,
            cancellationToken);
    }

    public Task<GitLabPackageProtectionRule> UpdateAsync(ProjectId projectId, long ruleId,
        UpdatePackageProtectionRuleRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            RulesRoute(projectId).Segment(ruleId).Build(),
            request,
            GitLabJsonContext.Default.UpdatePackageProtectionRuleRequest,
            GitLabJsonContext.Default.GitLabPackageProtectionRule,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long ruleId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(RulesRoute(projectId).Segment(ruleId).Build(), cancellationToken);
    }

    private static GitLabRouteBuilder RulesRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("protection")
            .Literal("rules");
    }
}