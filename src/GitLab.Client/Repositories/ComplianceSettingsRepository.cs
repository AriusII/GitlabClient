using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class ComplianceSettingsRepository(IGitLabApiConnection connection) : IComplianceSettingsRepository
{
    public Task<GitLabCompliancePolicySettings> GetAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("admin").Literal("security").Literal("compliance_policy_settings").Build(),
            GitLabJsonContext.Default.GitLabCompliancePolicySettings,
            cancellationToken);
    }

    public Task<GitLabCompliancePolicySettings> UpdateAsync(UpdateCompliancePolicySettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("admin").Literal("security").Literal("compliance_policy_settings").Build(),
            request,
            GitLabJsonContext.Default.UpdateCompliancePolicySettingsRequest,
            GitLabJsonContext.Default.GitLabCompliancePolicySettings,
            cancellationToken);
    }

    public Task SetExternalControlStatusAsync(ProjectId projectId, long controlId,
        SetComplianceExternalControlStatusRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("compliance_external_controls")
                .Segment(controlId).Literal("status").Build(),
            request,
            GitLabJsonContext.Default.SetComplianceExternalControlStatusRequest,
            cancellationToken);
    }
}