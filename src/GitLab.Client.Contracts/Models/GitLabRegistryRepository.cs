namespace GitLab.Client.Models;

/// <summary>
///     A container registry repository, as returned by
///     <c>GET /projects/:id/registry/repositories</c>, <c>GET /groups/:id/registry/repositories</c> and
///     <c>GET /registry/repositories/:id</c>.
/// </summary>
public sealed record GitLabRegistryRepository
{
    public required long Id { get; init; }

    /// <summary>The repository name within its project - typically empty for a project's default repository.</summary>
    public required string Name { get; init; }

    /// <summary>The repository path, e.g. <c>group/project</c> or <c>group/project/image</c>.</summary>
    public required string Path { get; init; }

    public required long ProjectId { get; init; }

    /// <summary>
    ///     The registry pull location, e.g. <c>registry.example.com:5000/group/project</c>. Not a
    ///     <see cref="Uri" />: GitLab's registry host:port notation has no scheme.
    /// </summary>
    public required string Location { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>When a cleanup policy run against this repository last started, if one ever has.</summary>
    public DateTimeOffset? CleanupPolicyStartedAt { get; init; }

    /// <summary>Present only when the request asked for <c>tags_count=true</c>.</summary>
    public int? TagsCount { get; init; }

    /// <summary>Present only when the request asked for <c>tags=true</c>.</summary>
    public IReadOnlyList<GitLabRegistryRepositoryTag>? Tags { get; init; }

    /// <summary>The relative API path that deletes this repository.</summary>
    public string? DeleteApiPath { get; init; }

    /// <summary>
    ///     Total size in bytes. Present only when the request asked for it (the single-repository endpoint's
    ///     <c>size=true</c>).
    /// </summary>
    public long? Size { get; init; }

    /// <summary>
    ///     The repository's pending-deletion state. GitLab currently reports <c>delete_scheduled</c>,
    ///     <c>delete_failed</c> or <c>delete_ongoing</c>, and omits the field otherwise. Left as a plain
    ///     string rather than an enum: the spec types it as a bare string with no enumerated vocabulary, and
    ///     a value the registry adds later must not turn a healthy response into a deserialization failure.
    /// </summary>
    public string? Status { get; init; }
}