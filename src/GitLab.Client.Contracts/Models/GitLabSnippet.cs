using System.Diagnostics.CodeAnalysis;

using GitLab.Client.Domain;

namespace GitLab.Client.Models;

/// <summary>
///     A snippet, as returned by both halves of the GitLab Snippets API - the personal surface
///     (<c>/snippets</c>) and the project surface (<c>/projects/:id/snippets</c>). The two entities are
///     declared separately in the spec (<c>APIEntitiesPersonalSnippet</c> and
///     <c>APIEntitiesProjectSnippet</c>) but are field-for-field identical, so one type models both.
/// </summary>
public sealed record GitLabSnippet
{
    /// <summary>The snippet id. Personal and project snippets share one id space.</summary>
    public required long Id { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    /// <summary>Who can see the snippet. GitLab defaults a personal snippet to <c>internal</c>.</summary>
    public GitLabVisibility? Visibility { get; init; }

    public GitLabUser? Author { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    ///     When GitLab will expire the snippet, or <c>null</c> when the snippet has no configured expiration.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>The owning project, or null for a personal snippet.</summary>
    public long? ProjectId { get; init; }

    public Uri? WebUrl { get; init; }

    /// <summary>
    ///     Where the raw content of the (first) file is served from. Fetching it needs the same credential
    ///     as any other call; prefer the client's raw-download methods over dereferencing this yourself.
    /// </summary>
    public Uri? RawUrl { get; init; }

    /// <summary>
    ///     The snippet repository's SSH clone URL. Deliberately a <see cref="string" /> rather than a
    ///     <see cref="Uri" />: an instance configured with an SCP-style remote answers
    ///     <c>git@host:snippets/65.git</c>, which is not a parsable absolute URI and would turn a healthy
    ///     response into a deserialization failure.
    /// </summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "An SCP-style remote (git@host:snippets/65.git) is not a parsable absolute URI, so Uri would turn a healthy response into a deserialization failure.")]
    public string? SshUrlToRepo { get; init; }

    /// <summary>The snippet repository's HTTP clone URL.</summary>
    public Uri? HttpUrlToRepo { get; init; }

    /// <summary>
    ///     The name of the first file. A legacy single-file view of <see cref="Files" />, kept by GitLab for
    ///     snippets created before multi-file support.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>Every file in the snippet repository, with the raw URL of each.</summary>
    public IReadOnlyList<GitLabSnippetFile>? Files { get; init; }

    public bool? Imported { get; init; }

    /// <summary>
    ///     Where the snippet was imported from, or <c>none</c>. Left as free text: the spec declares no
    ///     vocabulary for it, and a source GitLab adds later must not break deserialization.
    /// </summary>
    public string? ImportedFrom { get; init; }

    /// <summary>
    ///     The Gitaly storage shard holding the snippet repository. Free text - the set of shard names is
    ///     per-instance configuration, not a fixed enumeration.
    /// </summary>
    public string? RepositoryStorage { get; init; }
}