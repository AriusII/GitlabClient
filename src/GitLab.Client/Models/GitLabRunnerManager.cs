namespace GitLab.Client.Models;

/// <summary>
///     One runner manager - a single machine running <c>gitlab-runner</c> under a shared runner
///     registration (<c>GET /runners/:id/managers</c>). A runner created once can be started on many hosts;
///     each host reports itself as a manager, identified by its <see cref="SystemId" />.
/// </summary>
public sealed record GitLabRunnerManager
{
    public required long Id { get; init; }

    /// <summary>The manager's stable per-machine identifier, as sent by the runner itself.</summary>
    public string? SystemId { get; init; }

    public string? Version { get; init; }

    public string? Revision { get; init; }

    public string? Platform { get; init; }

    public string? Architecture { get; init; }

    /// <summary>Declared as a bare <c>string</c> in the spec, with no <c>format: date-time</c>; it is an ISO-8601 timestamp.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    ///     When this manager last contacted the instance. A bare <c>string</c> in the spec; an ISO-8601 timestamp on the
    ///     wire.
    /// </summary>
    public DateTimeOffset? ContactedAt { get; init; }

    public string? IpAddress { get; init; }

    /// <summary>"online", "offline", "stale" or "never_contacted".</summary>
    public string? Status { get; init; }

    /// <summary>
    ///     What the manager is doing right now. The spec enumerates "active" and "idle", while
    ///     <see cref="GitLabRunner.JobExecutionStatus" /> is documented as "idle" or "running"; the two
    ///     disagree, so this stays a string rather than an enum that could throw on an unlisted value.
    /// </summary>
    public string? JobExecutionStatus { get; init; }
}