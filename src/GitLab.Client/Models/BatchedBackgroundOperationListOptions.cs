using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for <c>GET /admin/batched_background_operations</c>.</summary>
[GitLabQuery]
public readonly record struct BatchedBackgroundOperationListOptions
{
    /// <summary>The database to list operations for. GitLab defaults to <c>main</c>.</summary>
    public GitLabBackgroundJobDatabase? Database { get; init; }

    /// <summary>Restricts the list to operations of this job class.</summary>
    public string? JobClassName { get; init; }
}