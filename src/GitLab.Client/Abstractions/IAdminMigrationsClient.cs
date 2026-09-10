using System.Text.Json;

using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's Rails schema migration admin APIs (<c>/admin/migrations/pending</c>,
///     <c>/admin/migrations/:timestamp/mark</c>) - listing migrations the instance has not yet run, and
///     marking one applied without actually running it, to skip a migration once an administrator has
///     determined it is safe to.
///     <para>
///         Every method here is available only to instance administrators; GitLab answers <c>403</c> to
///         anyone else. <see cref="ListPendingAsync" /> declares no response schema in GitLab's OpenAPI
///         document, so its answer comes back as a raw <see cref="JsonElement" /> rather than as an
///         invented DTO.
///     </para>
/// </summary>
public interface IAdminMigrationsClient
{
    /// <summary>Lists every pending (not yet run) migration for the instance.</summary>
    Task<JsonElement> ListPendingAsync(AdminMigrationListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Marks the migration identified by its version <paramref name="timestamp" /> as successfully
    ///     applied, without running it, so <c>db:migrate</c> tasks skip it. GitLab answers <c>422</c> if
    ///     the migration is not currently pending.
    /// </summary>
    Task MarkAppliedAsync(long timestamp, BackgroundJobDatabaseRequest? request = null,
        CancellationToken cancellationToken = default);
}