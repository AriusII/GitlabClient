using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for the references a commit was pushed to
///     (<c>GET /projects/:id/repository/commits/:sha/refs</c>).
/// </summary>
[GitLabQuery]
public sealed record CommitRefListOptions
{
    /// <summary>Restricts the answer to branches or to tags. GitLab returns both when unset.</summary>
    public GitLabCommitRefScope? Type { get; init; }

    public int? PerPage { get; init; }
}