namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/freeze_periods/:freeze_period_id</c>. Every member is
///     optional here - unlike the create form - so a caller can move just one boundary.
/// </summary>
public sealed record UpdateFreezePeriodRequest
{
    /// <summary>Start of the freeze window, in cron format.</summary>
    public string? FreezeStart { get; init; }

    /// <summary>End of the freeze window, in cron format.</summary>
    public string? FreezeEnd { get; init; }

    /// <summary>TZ database name the cron fields are read in.</summary>
    public string? CronTimezone { get; init; }
}