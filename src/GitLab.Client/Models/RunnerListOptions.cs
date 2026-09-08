using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters shared by every runner listing - <c>GET /runners</c>, <c>GET /runners/all</c>,
///     <c>GET /projects/:id/runners</c> and <c>GET /groups/:id/runners</c>.
///     <para>
///         GitLab's <c>scope</c> parameter is deliberately absent: the spec marks it "Deprecated: Use
///         <c>type</c> or <c>status</c> instead", and those two cover every value it accepted.
///     </para>
/// </summary>
[GitLabQuery]
public readonly record struct RunnerListOptions
{
    /// <summary>"instance_type", "group_type" or "project_type".</summary>
    public string? Type { get; init; }

    /// <summary>Return only runners that are ignoring new jobs (<see langword="true" />) or accepting them.</summary>
    public bool? Paused { get; init; }

    /// <summary>"active", "paused", "online", "offline", "never_contacted" or "stale".</summary>
    public string? Status { get; init; }

    /// <summary>Return only runners carrying all of these tags. Sent comma-joined, as GitLab's spec declares it.</summary>
    public IReadOnlyList<string>? TagList { get; init; }

    /// <summary>Return only runners whose version starts with this prefix - for example "17.".</summary>
    public string? VersionPrefix { get; init; }

    public int? PerPage { get; init; }
}