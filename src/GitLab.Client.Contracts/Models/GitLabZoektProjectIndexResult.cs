namespace GitLab.Client.Models;

/// <summary>
///     The result of triggering Zoekt indexing for one project (<c>PUT /admin/zoekt/projects/:project_id/index</c>).
/// </summary>
public sealed record GitLabZoektProjectIndexResult
{
    /// <summary>The ID of the background job GitLab enqueued to perform the indexing.</summary>
    public string? JobId { get; init; }
}