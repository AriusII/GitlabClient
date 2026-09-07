namespace GitLab.Client.Models;

/// <summary>
///     A deploy freeze window on a project (<c>/projects/:id/freeze_periods</c>) - the span during which
///     GitLab refuses to run deployment jobs.
///     <para>
///         <see cref="FreezeStart" /> and <see cref="FreezeEnd" /> are cron expressions, not timestamps:
///         a freeze period is recurring by nature, so <c>0 23 * * 5</c> / <c>0 7 * * 1</c> freezes every
///         weekend rather than one specific one. Both are interpreted in <see cref="CronTimezone" />.
///     </para>
/// </summary>
public sealed record GitLabFreezePeriod
{
    public required long Id { get; init; }

    /// <summary>Start of the freeze window, as a cron expression - for example <c>0 23 * * 5</c>.</summary>
    public required string FreezeStart { get; init; }

    /// <summary>End of the freeze window, as a cron expression - for example <c>0 7 * * 1</c>.</summary>
    public required string FreezeEnd { get; init; }

    /// <summary>
    ///     The time zone the two cron expressions are evaluated in, as a TZ database name
    ///     (<c>Etc/UTC</c>, <c>Europe/Berlin</c>). GitLab defaults it to <c>UTC</c>.
    /// </summary>
    public string? CronTimezone { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}