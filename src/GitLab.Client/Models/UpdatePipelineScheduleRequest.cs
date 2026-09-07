namespace GitLab.Client.Models;

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
}