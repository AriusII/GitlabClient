namespace GitLab.Client.Models;

/// <summary>
///     A pipeline schedule, as returned by the GitLab Pipeline schedules API
///     (<c>/projects/:id/pipeline_schedules</c>).
///     <para>
///         GitLab has two shapes for this entity: the list endpoint returns the summary form, while get,
///         create, update and take-ownership return the "details" form, which adds
///         <see cref="LastPipeline" />. Both are modelled here, so a member that exists only on the details
///         form is nullable and comes back <see langword="null" /> from <c>ListAsync</c>.
///     </para>
/// </summary>
public sealed record GitLabPipelineSchedule
{
    public required long Id { get; init; }

    public required string Description { get; init; }

    /// <summary>The branch or tag the scheduled pipeline runs against.</summary>
    public required string Ref { get; init; }

    /// <summary>The schedule itself, in cron syntax - for example <c>0 1 * * 5</c>.</summary>
    public required string Cron { get; init; }

    /// <summary>The timezone the <see cref="Cron" /> expression is interpreted in; GitLab defaults it to UTC.</summary>
    public string? CronTimezone { get; init; }

    public DateTimeOffset? NextRunAt { get; init; }

    /// <summary>Whether the schedule is currently armed. An inactive schedule keeps its cron but never fires.</summary>
    public bool? Active { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>The user the scheduled pipelines run as - changed with <c>TakeOwnershipAsync</c>.</summary>
    public GitLabUser? Owner { get; init; }

    /// <summary>
    ///     The schedule input returned by GitLab. The pinned GitLab 19.4 schema models this response member
    ///     as a single <c>APIEntitiesCiInput</c> object, while create and update accept an array of inputs.
    /// </summary>
    public GitLabPipelineInput? Inputs { get; init; }

    /// <summary>
    ///     The schedule variable returned by GitLab. The pinned GitLab 19.4 schema models this response
    ///     member as one <c>APIEntitiesCiVariable</c> object; use the dedicated variable endpoints to manage
    ///     the collection.
    /// </summary>
    public GitLabVariable? Variables { get; init; }

    /// <summary>
    ///     The most recent pipeline this schedule triggered. Part of GitLab's "details" shape only, so it is
    ///     <see langword="null" /> on items returned by <c>ListAsync</c>.
    /// </summary>
    public GitLabPipeline? LastPipeline { get; init; }
}