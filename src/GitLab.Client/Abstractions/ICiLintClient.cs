using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "CI lint" API area (<c>/projects/:id/ci/lint</c>): validate the configuration a
///     project has committed, or validate YAML you are about to commit, in that project's context.
///     <para>
///         Both operations answer <c>200 OK</c> even for an invalid configuration - the verdict is
///         <see cref="GitLabCiLintResult.Valid" /> plus <see cref="GitLabCiLintResult.Errors" />, never a
///         4xx. The absence of a <c>GitLabApiException</c> does not mean the YAML is good.
///     </para>
/// </summary>
public interface ICiLintClient
{
    /// <summary>Validates the <c>.gitlab-ci.yml</c> already committed to the project.</summary>
    Task<GitLabCiLintResult> ValidateProjectConfigurationAsync(ProjectId projectId, CiLintOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Validates supplied CI/CD configuration content in the context of the project.</summary>
    Task<GitLabCiLintResult> ValidateAsync(ProjectId projectId, ValidateCiConfigurationRequest request,
        CancellationToken cancellationToken = default);
}