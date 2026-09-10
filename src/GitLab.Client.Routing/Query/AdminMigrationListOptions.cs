using GitLab.Client.Models;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for <c>GET /admin/migrations/pending</c>.</summary>
[GitLabQuery]
public readonly record struct AdminMigrationListOptions
{
    /// <summary>The database to list pending migrations for. GitLab defaults to <c>main</c>.</summary>
    public GitLabBackgroundJobDatabase? Database { get; init; }
}