namespace GitLab.Client.Models;

/// <summary>
///     A container registry tag, as returned either embedded in a <see cref="GitLabRegistryRepository" />
///     (when a listing or detail call asks for <c>tags=true</c>) or directly by
///     <c>GET /projects/:id/registry/repositories/:repository_id/tags</c>.
/// </summary>
public sealed record GitLabRegistryRepositoryTag
{
    public required string Name { get; init; }

    /// <summary>The full tag reference, e.g. <c>group/project:latest</c>.</summary>
    public required string Path { get; init; }

    /// <summary>
    ///     The registry pull location for this tag, e.g. <c>registry.example.com:5000/group/project:latest</c>.
    ///     Not a <see cref="Uri" />: GitLab's registry host:port notation has no scheme.
    /// </summary>
    public required string Location { get; init; }
}