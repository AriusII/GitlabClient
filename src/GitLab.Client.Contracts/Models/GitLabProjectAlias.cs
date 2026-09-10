namespace GitLab.Client.Models;

/// <summary>
///     A project alias, as returned by the GitLab Project alias API (<c>/project_aliases</c>) - a second
///     name a project answers to over Git-over-SSH/HTTP, used when a repository is mirrored under a name
///     that differs from its GitLab path.
/// </summary>
/// <remarks>The whole area is administrator-only and available on GitLab Premium and Ultimate only.</remarks>
public sealed record GitLabProjectAlias
{
    /// <summary>The alias's own numeric id. Note that the alias routes address an alias by name, not by this.</summary>
    public required long Id { get; init; }

    /// <summary>The numeric id of the project the alias points at.</summary>
    public required long ProjectId { get; init; }

    /// <summary>The alias itself - the name Git clients may use in place of the project's path.</summary>
    public required string Name { get; init; }
}