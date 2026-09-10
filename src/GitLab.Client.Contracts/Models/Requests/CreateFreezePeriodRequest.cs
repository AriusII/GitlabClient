namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/freeze_periods</c>. Both boundaries are cron expressions,
///     which is what makes a freeze period recurring; the spec marks them required.
/// </summary>
public sealed record CreateFreezePeriodRequest
{
    /// <summary>Start of the freeze window, in cron format - for example <c>0 23 * * 5</c>.</summary>
    public required string FreezeStart { get; init; }

    /// <summary>End of the freeze window, in cron format - for example <c>0 7 * * 1</c>.</summary>
    public required string FreezeEnd { get; init; }

    /// <summary>TZ database name the cron fields are read in. GitLab defaults to <c>UTC</c> when omitted.</summary>
    public string? CronTimezone { get; init; }
}