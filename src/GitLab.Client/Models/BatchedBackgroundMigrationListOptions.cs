using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for <c>GET /admin/batched_background_migrations</c>.</summary>
[GitLabQuery]
public readonly record struct BatchedBackgroundMigrationListOptions
{
    /// <summary>The database to list migrations for. GitLab defaults to <c>main</c>.</summary>
    public GitLabBackgroundJobDatabase? Database { get; init; }

    /// <summary>Restricts the list to migrations of this job class.</summary>
    public string? JobClassName { get; init; }
}