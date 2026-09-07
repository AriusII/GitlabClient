using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Freeze periods" API area (<c>/projects/:id/freeze_periods</c>) - the deploy
///     freeze windows during which GitLab refuses to run a project's deployment jobs.
///     <para>
///         A freeze window is described by two cron expressions rather than two timestamps, so it
///         recurs: <c>0 23 * * 5</c> to <c>0 7 * * 1</c> freezes every weekend. Reading requires the
///         Reporter role; creating, updating and deleting require Maintainer.
///     </para>
/// </summary>
public interface IFreezePeriodsClient
{
    /// <summary>Streams every freeze period configured on the project.</summary>
    IAsyncEnumerable<GitLabFreezePeriod> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one freeze period by its numeric id.</summary>
    Task<GitLabFreezePeriod> GetAsync(ProjectId projectId, long freezePeriodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a freeze period. Both cron boundaries are required; the time zone defaults to UTC when
    ///     <see cref="CreateFreezePeriodRequest.CronTimezone" /> is left null.
    /// </summary>
    Task<GitLabFreezePeriod> CreateAsync(ProjectId projectId, CreateFreezePeriodRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates one or both boundaries, or the time zone, of an existing freeze period.</summary>
    Task<GitLabFreezePeriod> UpdateAsync(ProjectId projectId, long freezePeriodId,
        UpdateFreezePeriodRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a freeze period, immediately lifting the freeze it described.</summary>
    Task DeleteAsync(ProjectId projectId, long freezePeriodId, CancellationToken cancellationToken = default);
}