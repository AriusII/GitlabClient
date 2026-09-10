namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /runners/:id</c>. Every member is optional - omitted properties are not sent,
///     so they keep their current server-side value.
///     <para>
///         GitLab's <c>active</c> flag is deliberately absent: the spec marks it "Deprecated: Use
///         <c>paused</c> instead", and the two are mutually exclusive, so exposing both would let a caller
///         build a request GitLab rejects.
///     </para>
/// </summary>
public sealed record UpdateRunnerRequest
{
    public string? Description { get; init; }

    /// <summary>Whether the runner should ignore new jobs.</summary>
    public bool? Paused { get; init; }

    /// <summary>Replaces the runner's tags wholesale; this is not a merge.</summary>
    public IReadOnlyList<string>? TagList { get; init; }

    /// <summary>Whether the runner may pick up jobs that carry no tags.</summary>
    public bool? RunUntagged { get; init; }

    /// <summary>Whether the runner refuses to be assigned to further projects.</summary>
    public bool? Locked { get; init; }

    /// <summary>"not_protected" or "ref_protected".</summary>
    public string? AccessLevel { get; init; }

    /// <summary>Maximum time in seconds a job may run on this runner.</summary>
    public int? MaximumTimeout { get; init; }

    /// <summary>Free-form maintenance note, up to 1024 characters.</summary>
    public string? MaintenanceNote { get; init; }
}