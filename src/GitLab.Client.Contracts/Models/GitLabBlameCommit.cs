namespace GitLab.Client.Models;

/// <summary>
///     The commit that last touched a run of lines, as embedded in a <see cref="GitLabBlameRange" />.
/// </summary>
/// <remarks>
///     Deliberately not <see cref="GitLabCommit" />: the blame payload is a narrower entity that carries
///     no <c>short_id</c>, <c>title</c> or <c>web_url</c>, all of which <see cref="GitLabCommit" />
///     requires.
/// </remarks>
public sealed record GitLabBlameCommit
{
    public required string Id { get; init; }

    /// <summary>The commit's parents; two or more for a merge commit.</summary>
    public IReadOnlyList<string>? ParentIds { get; init; }

    public string? Message { get; init; }

    public string? AuthorName { get; init; }

    public string? AuthorEmail { get; init; }

    public DateTimeOffset? AuthoredDate { get; init; }

    public string? CommitterName { get; init; }

    public string? CommitterEmail { get; init; }

    public DateTimeOffset? CommittedDate { get; init; }
}