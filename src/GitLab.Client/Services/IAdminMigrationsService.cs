using System.Text.Json;

using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Migrations, sitting between the public
///     <c>IAdminMigrationsClient</c> controller and <c>IAdminMigrationsRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this
///     is the seam where request validation, caching, or cross-resource composition would go once the
///     resource needs more than pass-through.
/// </summary>
internal interface IAdminMigrationsService
{
    Task<JsonElement> ListPendingAsync(AdminMigrationListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task MarkAppliedAsync(long timestamp, BackgroundJobDatabaseRequest? request = null,
        CancellationToken cancellationToken = default);
}