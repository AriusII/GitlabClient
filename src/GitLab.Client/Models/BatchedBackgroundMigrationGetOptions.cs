using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for <c>GET /admin/batched_background_migrations/:id</c>.</summary>
[GitLabQuery]
public readonly record struct BatchedBackgroundMigrationGetOptions
{
    /// <summary>The database the migration id belongs to. GitLab defaults to <c>main</c>.</summary>
    public GitLabBackgroundJobDatabase? Database { get; init; }
}