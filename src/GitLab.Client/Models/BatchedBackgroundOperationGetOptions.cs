using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for <c>GET /admin/batched_background_operations/:id</c>.</summary>
[GitLabQuery]
public sealed record BatchedBackgroundOperationGetOptions
{
    /// <summary>The database the operation id belongs to. GitLab defaults to <c>main</c>.</summary>
    public GitLabBackgroundJobDatabase? Database { get; init; }
}