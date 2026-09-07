namespace GitLab.Client.Models;

/// <summary>
///     Where a project's repository physically lives - the response of <c>GET /projects/:id/storage</c>,
///     an administrator-only endpoint.
/// </summary>
public sealed record GitLabProjectStorage
{
    public required long ProjectId { get; init; }

    /// <summary>The path of the repository on the Gitaly node, relative to that node's storage root.</summary>
    public string? DiskPath { get; init; }

    /// <summary>The name of the Gitaly storage shard holding the repository.</summary>
    public string? RepositoryStorage { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}