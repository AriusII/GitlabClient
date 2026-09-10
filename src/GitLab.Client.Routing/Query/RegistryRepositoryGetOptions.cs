using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Inclusion flags for <c>GET /registry/repositories/:id</c>.</summary>
[GitLabQuery]
public readonly record struct RegistryRepositoryGetOptions
{
    /// <summary>Include the repository's tags in the response.</summary>
    public bool? Tags { get; init; }

    /// <summary>Include the repository's tag count in the response.</summary>
    public bool? TagsCount { get; init; }

    /// <summary>Include the repository's total size in bytes in the response.</summary>
    public bool? Size { get; init; }
}