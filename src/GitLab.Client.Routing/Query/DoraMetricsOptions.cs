using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     The optional filters shared by <c>GET /groups/:id/dora/metrics</c> and
///     <c>GET /projects/:id/dora/metrics</c>. The endpoints' required <c>metric</c> parameter is an
///     ordinary method parameter rather than a member here, since <c>[GitLabQuery]</c> properties must
///     all be nullable (GLQ0002) and it is never omitted.
/// </summary>
[GitLabQuery]
public readonly record struct DoraMetricsOptions
{
    /// <summary>Start of the date range, inclusive. Defaults to 3 months ago when omitted.</summary>
    public string? StartDate { get; init; }

    /// <summary>End of the date range, inclusive. Defaults to the current date when omitted.</summary>
    public string? EndDate { get; init; }

    /// <summary>Bucket size for the series - <c>all</c>, <c>monthly</c>, <c>daily</c>.</summary>
    public string? Interval { get; init; }

    /// <summary>Limits the metric to deployments on environments with these tiers.</summary>
    public IReadOnlyList<string>? EnvironmentTiers { get; init; }
}