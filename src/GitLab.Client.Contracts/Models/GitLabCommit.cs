namespace GitLab.Client.Models;

/// <summary>
///     A commit in a project's repository. GitLab returns this shape at several depths: the compact form
///     embedded in a branch, tag or tree entry carries only the identity fields, while the detailed form
///     from <c>POST /projects/:id/repository/commits</c> and
///     <c>GET /projects/:id/repository/commits/:sha</c> also fills <see cref="Stats" />,
///     <see cref="Status" />, <see cref="ProjectId" /> and <see cref="LastPipeline" />. Everything past
///     the identity fields is therefore nullable.
/// </summary>
public sealed record GitLabCommit
{
    public required string Id { get; init; }

    public required string ShortId { get; init; }

    public required string Title { get; init; }

    public string? Message { get; init; }

    public string? AuthorName { get; init; }

    public string? AuthorEmail { get; init; }

    public DateTimeOffset? AuthoredDate { get; init; }

    public string? CommitterName { get; init; }

    public string? CommitterEmail { get; init; }

    public DateTimeOffset? CommittedDate { get; init; }

    public required Uri WebUrl { get; init; }

    /// <summary>When the commit object was created; the same instant as <see cref="CommittedDate" />.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>The commit's parents. Empty for a root commit, two or more for a merge.</summary>
    public IReadOnlyList<string>? ParentIds { get; init; }

    /// <summary>
    ///     The Git trailers parsed out of the commit message, one value per key. GitLab keeps only the
    ///     last value when a trailer is repeated; <see cref="ExtendedTrailers" /> keeps them all.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Trailers { get; init; }

    /// <summary>Every value of every Git trailer, including repeats such as multiple sign-offs.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>>? ExtendedTrailers { get; init; }

    /// <summary>Line counts for the commit. Only the detailed commit payloads carry it.</summary>
    public GitLabCommitStats? Stats { get; init; }

    /// <summary>
    ///     The commit's CI status - <c>success</c>, <c>failed</c>, <c>running</c> and so on - or
    ///     <see langword="null" /> when no pipeline has run for it. GitLab keeps extending this
    ///     vocabulary, so it stays a string.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>The project the commit belongs to.</summary>
    public long? ProjectId { get; init; }

    /// <summary>The most recent pipeline run for this commit, when one exists.</summary>
    public GitLabPipeline? LastPipeline { get; init; }
}