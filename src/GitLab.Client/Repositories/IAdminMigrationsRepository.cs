using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Migrations resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IAdminMigrationsService), typeof(IAdminMigrationsClient))]
internal interface IAdminMigrationsRepository
{
    Task<JsonElement> ListPendingAsync(AdminMigrationListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task MarkAppliedAsync(long timestamp, BackgroundJobDatabaseRequest? request = null,
        CancellationToken cancellationToken = default);
}