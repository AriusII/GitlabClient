using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     The optional filters for <c>GET /projects/:id/analytics/deployment_frequency</c>. The
///     endpoint's required <c>environment</c> and <c>from</c> parameters are ordinary method
///     parameters rather than members here, since <c>[GitLabQuery]</c> properties must all be
///     nullable (GLQ0002) and these two are never omitted.
/// </summary>
[GitLabQuery]
public readonly record struct DeploymentFrequencyListOptions
{
    /// <summary>End of the date range. Defaults to the current date when omitted.</summary>
    public string? To { get; init; }

    /// <summary>Bucket size for the series - e.g. <c>all</c>, <c>monthly</c>, <c>daily</c>.</summary>
    public string? Interval { get; init; }
}