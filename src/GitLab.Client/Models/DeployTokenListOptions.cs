using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for the deploy token listings (<c>GET /deploy_tokens</c>,
///     <c>GET /projects/:id/deploy_tokens</c>, <c>GET /groups/:id/deploy_tokens</c>), whose parameter
///     set GitLab declares identically for all three.
/// </summary>
[GitLabQuery]
public sealed record DeployTokenListOptions
{
    /// <summary>
    ///     Return only active tokens (<see langword="true" />), or only revoked and expired ones
    ///     (<see langword="false" />). Omit for all of them.
    /// </summary>
    public bool? Active { get; init; }

    public int? PerPage { get; init; }
}