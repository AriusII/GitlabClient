using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class AdminMigrationsRepository(IGitLabApiConnection connection) : IAdminMigrationsRepository
{
    public Task<JsonElement> ListPendingAsync(AdminMigrationListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("admin").Literal("migrations").Literal("pending").QueryFrom(options).Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task MarkAppliedAsync(long timestamp, BackgroundJobDatabaseRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("admin").Literal("migrations").Segment(timestamp).Literal("mark").Build(),
            request ?? new BackgroundJobDatabaseRequest(),
            GitLabJsonContext.Default.BackgroundJobDatabaseRequest,
            cancellationToken);
    }
}