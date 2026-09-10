namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/pipeline_schedules/:pipeline_schedule_id</c>. Every member is
///     optional - omitted properties are not sent, so they keep their current server-side value - and GitLab
///     recomputes the next run time after any change.
/// </summary>
public sealed record UpdatePipelineScheduleRequest
{
    public string? Description { get; init; }

    /// <summary>The branch or tag the scheduled pipeline runs against.</summary>
    public string? Ref { get; init; }

    /// <summary>The schedule in cron syntax - for example <c>0 1 * * 5</c>.</summary>
    public string? Cron { get; init; }

    public string? CronTimezone { get; init; }

    public bool? Active { get; init; }

    /// <summary>
    ///     Inputs to add, update, or delete. Set <see cref="GitLabPipelineInput.Destroy" /> on an entry to
    ///     remove it; entries left out of this collection are unchanged.
    /// </summary>
    public IReadOnlyList<GitLabPipelineInput>? Inputs { get; init; }
}