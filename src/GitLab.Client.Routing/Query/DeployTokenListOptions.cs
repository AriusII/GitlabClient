using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for the deploy token listings (<c>GET /deploy_tokens</c>,
///     <c>GET /projects/:id/deploy_tokens</c>, <c>GET /groups/:id/deploy_tokens</c>), whose parameter
///     set GitLab declares identically for all three.
/// </summary>
[GitLabQuery]
public readonly record struct DeployTokenListOptions
{
    /// <summary>
    ///     Return only active tokens (<see langword="true" />), or only revoked and expired ones
    ///     (<see langword="false" />). Omit for all of them.
    /// </summary>
    public bool? Active { get; init; }

    /// <summary>The one-based result page to retrieve.</summary>
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}