namespace GitLab.Client.Models;

/// <summary>
///     A project's CI/CD job token access settings (<c>GET /projects/:id/job_token_scope</c>) - whether
///     job tokens are restricted going in and out of the project.
/// </summary>
public sealed record GitLabProjectJobTokenScope
{
    /// <summary>
    ///     Whether the project restricts which other projects and groups can authenticate against it with a
    ///     CI/CD job token minted elsewhere.
    /// </summary>
    public bool? InboundEnabled { get; init; }

    /// <summary>
    ///     Whether the project restricts which other projects and groups a CI/CD job token minted here can
    ///     authenticate against.
    /// </summary>
    public bool? OutboundEnabled { get; init; }
}