using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the CI Lint resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ICiLintService), typeof(ICiLintClient))]
internal interface ICiLintRepository
{
    Task<GitLabCiLintResult> ValidateProjectConfigurationAsync(ProjectId projectId, CiLintOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabCiLintResult> ValidateAsync(ProjectId projectId, ValidateCiConfigurationRequest request,
        CancellationToken cancellationToken = default);
}