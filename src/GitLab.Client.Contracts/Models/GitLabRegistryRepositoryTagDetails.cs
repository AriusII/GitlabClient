namespace GitLab.Client.Models;

/// <summary>
///     Full detail of one container registry tag, as returned by
///     <c>GET /projects/:id/registry/repositories/:repository_id/tags/:tag_name</c>. The plain listing
///     endpoint returns the narrower <see cref="GitLabRegistryRepositoryTag" /> instead.
/// </summary>
public sealed record GitLabRegistryRepositoryTagDetails
{
    public required string Name { get; init; }

    /// <summary>The full tag reference, e.g. <c>group/project:latest</c>.</summary>
    public required string Path { get; init; }

    /// <summary>
    ///     The registry pull location for this tag, e.g. <c>registry.example.com:5000/group/project:latest</c>.
    ///     Not a <see cref="Uri" />: GitLab's registry host:port notation has no scheme.
    /// </summary>
    public required string Location { get; init; }

    /// <summary>The full manifest digest this tag currently points at, e.g. <c>sha256:4b6...</c>.</summary>
    public string? Revision { get; init; }

    /// <summary>The short form of <see cref="Revision" />.</summary>
    public string? ShortRevision { get; init; }

    /// <summary>The content-addressable image digest, e.g. <c>sha256:4b6...</c>.</summary>
    public string? Digest { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>Total size in bytes of the image this tag points at.</summary>
    public long? TotalSize { get; init; }
}