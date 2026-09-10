using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class TerraformStatesClient(IGitLabApiConnection connection) : ITerraformStatesClient
{
    public Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, string name, string? lockId = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            StateRoute(projectId, name).Query("ID", lockId).Build(),
            cancellationToken);
    }

    public Task UploadAsync(ProjectId projectId, string name, GitLabFileUpload state,
        CancellationToken cancellationToken = default)
    {
        return connection.PostFileAsync(
            StateRoute(projectId, name).Build(),
            state,
            null,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(StateRoute(projectId, name).Build(), cancellationToken);
    }

    public Task AuthorizeUploadAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            StateRoute(projectId, name).Literal("authorize").Build(),
            cancellationToken);
    }

    public Task LockAsync(ProjectId projectId, string name, LockTerraformStateRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            StateRoute(projectId, name).Literal("lock").Build(),
            request,
            GitLabJsonContext.Default.LockTerraformStateRequest,
            cancellationToken);
    }

    public Task UnlockAsync(ProjectId projectId, string name, string? lockId = null,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            StateRoute(projectId, name).Literal("lock").Query("ID", lockId).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadVersionAsync(ProjectId projectId, string name, long serial,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            StateRoute(projectId, name).Literal("versions").Segment(serial).Build(),
            cancellationToken);
    }

    public Task DeleteVersionAsync(ProjectId projectId, string name, long serial,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            StateRoute(projectId, name).Literal("versions").Segment(serial).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabTerraformStateProtectionRule> ListProtectionRulesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProtectionRulesRoute(projectId).Build(),
            GitLabJsonContext.Default.GitLabTerraformStateProtectionRuleArray,
            cancellationToken);
    }

    public Task<GitLabTerraformStateProtectionRule> CreateProtectionRuleAsync(ProjectId projectId,
        CreateTerraformStateProtectionRuleRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProtectionRulesRoute(projectId).Build(),
            request,
            GitLabJsonContext.Default.CreateTerraformStateProtectionRuleRequest,
            GitLabJsonContext.Default.GitLabTerraformStateProtectionRule,
            cancellationToken);
    }

    public Task<GitLabTerraformStateProtectionRule> UpdateProtectionRuleAsync(ProjectId projectId, long ruleId,
        UpdateTerraformStateProtectionRuleRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            ProtectionRulesRoute(projectId).Segment(ruleId).Build(),
            request,
            GitLabJsonContext.Default.UpdateTerraformStateProtectionRuleRequest,
            GitLabJsonContext.Default.GitLabTerraformStateProtectionRule,
            cancellationToken);
    }

    public Task DeleteProtectionRuleAsync(ProjectId projectId, long ruleId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ProtectionRulesRoute(projectId).Segment(ruleId).Build(),
            cancellationToken);
    }

    /// <summary>
    ///     The state name is caller-supplied free text and legally contains <c>/</c> and <c>.</c>
    ///     (<c>env/production.tfstate</c>), so it is <c>Escaped</c>, never <c>Literal</c> - an unencoded
    ///     slash would silently address a route that does not exist and read back as a missing state.
    /// </summary>
    private static GitLabRouteBuilder StateRoute(ProjectId projectId, string name)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("terraform").Literal("state")
            .Escaped(name);
    }

    private static GitLabRouteBuilder ProtectionRulesRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("terraform")
            .Literal("state_protection_rules");
    }
}