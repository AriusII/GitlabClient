namespace GitLab.Client.Models;

/// <summary>
///     A CI/CD runner, as returned by the GitLab Runners API (<c>/runners</c>, <c>/projects/:id/runners</c>,
///     <c>/groups/:id/runners</c>).
///     <para>
///         GitLab has two shapes for this entity: the list endpoints return the summary form, while
///         <c>GET /runners/:id</c>, <c>PUT /runners/:id</c> and both delete endpoints return the "details"
///         form. Both are modelled here, so every member below <see cref="JobExecutionStatus" /> is
///         detail-only and comes back <see langword="null" /> from a list call.
///     </para>
/// </summary>
public sealed record GitLabRunner
{
    public required long Id { get; init; }

    public string? Description { get; init; }

    public string? IpAddress { get; init; }

    /// <summary>
    ///     Legacy runner activity flag returned by GitLab. New consumers should use <see cref="Paused" />;
    ///     GitLab retains this field on response payloads for compatibility.
    /// </summary>
    public bool? Active { get; init; }

    /// <summary>Whether the runner is ignoring new jobs. This is the current flag; <c>active</c> is deprecated.</summary>
    public bool? Paused { get; init; }

    public bool? IsShared { get; init; }

    /// <summary>"instance_type", "group_type" or "project_type".</summary>
    public string? RunnerType { get; init; }

    public string? Name { get; init; }

    public bool? Online { get; init; }

    public GitLabUser? CreatedBy { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>"active", "paused", "online", "offline", "never_contacted" or "stale".</summary>
    public string? Status { get; init; }

    /// <summary>What the runner is doing right now - "idle" or "running".</summary>
    public string? JobExecutionStatus { get; init; }

    /// <summary>
    ///     The runner's tags. The OpenAPI document types this <c>string</c> on the response, which is wrong:
    ///     GitLab sends a JSON array, exactly as the update request body declares it.
    /// </summary>
    public IReadOnlyList<string>? TagList { get; init; }

    /// <summary>Whether the runner picks up jobs that carry no tags. Typed <c>string</c> in the spec; a boolean on the wire.</summary>
    public bool? RunUntagged { get; init; }

    /// <summary>
    ///     Whether the runner refuses to be assigned to further projects. Typed <c>string</c> in the spec; a boolean on
    ///     the wire.
    /// </summary>
    public bool? Locked { get; init; }

    /// <summary>Job timeout in seconds. Typed <c>string</c> in the spec; an integer on the wire.</summary>
    public int? MaximumTimeout { get; init; }

    /// <summary>"not_protected" or "ref_protected".</summary>
    public string? AccessLevel { get; init; }

    public string? Version { get; init; }

    public string? Revision { get; init; }

    public string? Platform { get; init; }

    public string? Architecture { get; init; }

    /// <summary>
    ///     When the runner last contacted the instance. Declared as a bare <c>string</c> in the spec, with no
    ///     <c>format: date-time</c>, but it is an ISO-8601 timestamp.
    /// </summary>
    public DateTimeOffset? ContactedAt { get; init; }

    public string? MaintenanceNote { get; init; }

    /// <summary>
    ///     Projects assigned to this runner. Present on the runner-details response and omitted from summary
    ///     listings.
    /// </summary>
    public IReadOnlyList<GitLabProject>? Projects { get; init; }

    /// <summary>
    ///     Groups assigned to this runner. Present on the runner-details response and omitted from summary
    ///     listings.
    /// </summary>
    public IReadOnlyList<GitLabRunnerGroup>? Groups { get; init; }
}