namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/pipeline_schedules</c>.</summary>
public sealed record CreatePipelineScheduleRequest
{
    public required string Description { get; init; }

    /// <summary>The branch or tag the scheduled pipeline runs against.</summary>
    public required string Ref { get; init; }

    /// <summary>The schedule in cron syntax - for example <c>0 1 * * 5</c>.</summary>
    public required string Cron { get; init; }

    /// <summary>Optional: the timezone the cron expression is read in. GitLab defaults to UTC when omitted.</summary>
    public string? CronTimezone { get; init; }

    /// <summary>Optional: whether the schedule is armed on creation. GitLab defaults to <see langword="true" />.</summary>
    public bool? Active { get; init; }

    /// <summary>
    ///     The CI/CD inputs this schedule supplies each time it starts a pipeline. GitLab permits string,
    ///     numeric, Boolean, array, and explicit <c>null</c> input values.
    /// </summary>
    public IReadOnlyList<GitLabPipelineInput>? Inputs { get; init; }
}