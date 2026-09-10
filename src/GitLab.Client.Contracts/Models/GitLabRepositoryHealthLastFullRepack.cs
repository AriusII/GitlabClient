namespace GitLab.Client.Models;

/// <summary>
///     When the repository was last fully repacked, from <c>GET /projects/:id/repository/health</c>.
///     Gitaly reports it as a protobuf timestamp rather than an ISO-8601 string, so it arrives split into
///     whole seconds plus a nanosecond remainder.
/// </summary>
public sealed record GitLabRepositoryHealthLastFullRepack
{
    /// <summary>Whole seconds since the Unix epoch.</summary>
    public long? Seconds { get; init; }

    /// <summary>The sub-second remainder, in nanoseconds.</summary>
    public long? Nanos { get; init; }
}