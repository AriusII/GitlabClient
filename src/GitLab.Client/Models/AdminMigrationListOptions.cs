using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for <c>GET /admin/migrations/pending</c>.</summary>
[GitLabQuery]
public sealed record AdminMigrationListOptions
{
    /// <summary>The database to list pending migrations for. GitLab defaults to <c>main</c>.</summary>
    public GitLabBackgroundJobDatabase? Database { get; init; }
}