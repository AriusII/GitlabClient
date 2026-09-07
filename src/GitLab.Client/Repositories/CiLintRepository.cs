using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class CiLintRepository(IGitLabApiConnection connection) : ICiLintRepository
{
    /// <summary>
    ///     A single object, not a collection - <c>GetAsync</c> rather than <c>GetPagedAsync</c>, which would
    ///     try to read the lint result as an array.
    /// </summary>
    public Task<GitLabCiLintResult> ValidateProjectConfigurationAsync(ProjectId projectId,
        CiLintOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("ci")
                .Literal("lint")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabCiLintResult,
            cancellationToken);
    }

    public Task<GitLabCiLintResult> ValidateAsync(ProjectId projectId, ValidateCiConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("ci")
                .Literal("lint")
                .Build(),
            request,
            GitLabJsonContext.Default.ValidateCiConfigurationRequest,
            GitLabJsonContext.Default.GitLabCiLintResult,
            cancellationToken);
    }
}