namespace GitLab.Client.Models;

/// <summary>
///     The Job Router endpoint advertised to an authenticated runner by
///     <c>GET /runners/router/discovery</c>.
/// </summary>
public sealed record GitLabRunnerRouterDiscovery
{
    /// <summary>The secure WebSocket endpoint for the GitLab Agent Server (KAS) Job Router.</summary>
    public Uri? ServerUrl { get; init; }
}