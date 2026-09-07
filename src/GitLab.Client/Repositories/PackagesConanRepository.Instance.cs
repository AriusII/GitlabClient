using GitLab.Client.Abstractions;
using GitLab.Client.Domain;

namespace GitLab.Client.Repositories;

/// <summary>
///     Repository health and credential checks (v1): <c>ping</c>, <c>users/authenticate</c> and
///     <c>users/check_credentials</c>, at both instance-wide and per-project scope.
///     <para>
///         GitLab declares no response schema for any of these three. <c>ping</c> is a pure status-code
///         check, so it goes through the body-less GET. <c>authenticate</c> and <c>check_credentials</c>
///         both return a value the caller does need (a Bearer token; a credentials confirmation) whose
///         content type GitLab does not document, so both are read back through
///         <see cref="IGitLabApiConnection.GetFileAsync" /> rather than guessed at as JSON.
///     </para>
/// </summary>
internal sealed partial class PackagesConanRepository
{
    private const string UsersSegment = "users";

    public Task PingAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(InstanceRoute().Literal("ping").Build(), cancellationToken);
    }

    public Task PingForProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(ProjectRoute(projectId).Literal("ping").Build(), cancellationToken);
    }

    public Task<GitLabFileResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            InstanceRoute().Literal(UsersSegment).Literal("authenticate").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> AuthenticateForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectRoute(projectId).Literal(UsersSegment).Literal("authenticate").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> CheckCredentialsAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            InstanceRoute().Literal(UsersSegment).Literal("check_credentials").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> CheckCredentialsForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectRoute(projectId).Literal(UsersSegment).Literal("check_credentials").Build(),
            cancellationToken);
    }
}