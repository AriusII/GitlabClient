using GitLab.Client.Domain;

namespace GitLab.Client.Models;

/// <summary>
///     A GitLab project (repository), as returned by <c>GET /projects/:id</c> and most of the other
///     Projects endpoints, and embedded - in this same reduced shape - by other resources such as
///     <see cref="GitLabGroup" />.
///     <para>
///         GitLab's project resource carries well over a hundred fields; only the handful every embedding
///         actually returns are modelled here and, of those, only the five GitLab always includes are
///         <c>required</c>. Treat this as the common subset rather than an exhaustive shape.
///     </para>
/// </summary>
public sealed record GitLabProject
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required string PathWithNamespace { get; init; }

    public string? Description { get; init; }

    public required GitLabVisibility Visibility { get; init; }

    public required Uri WebUrl { get; init; }

    public string? DefaultBranch { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public bool? Archived { get; init; }

    public int? StarCount { get; init; }

    public int? ForksCount { get; init; }
}