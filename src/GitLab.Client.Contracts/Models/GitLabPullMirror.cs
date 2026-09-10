using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     A project's pull-mirror configuration and status, as returned by the Project mirrors API
///     (<c>/projects/:id/mirror/pull</c>). Unlike <see cref="GitLabRemoteMirror" /> (a project can have
///     several push mirrors), a project has at most one pull mirror.
/// </summary>
public sealed record GitLabPullMirror
{
    public required long Id { get; init; }

    /// <summary>Free-text status - e.g. <c>none</c>, <c>scheduled</c>, <c>started</c>, <c>finished</c>, <c>failed</c>.</summary>
    public string? UpdateStatus { get; init; }

    /// <summary>
    ///     The upstream URL the project pulls from. Deliberately a <see cref="string" /> rather than a
    ///     <see cref="Uri" />: GitLab scrubs credentials out of every response the same way it does for
    ///     <see cref="GitLabRemoteMirror.Url" />, returning literally
    ///     <c>https://*****:*****@example.com/gitlab/example.git</c>, and that userinfo is not reliably
    ///     parseable.
    /// </summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "GitLab scrubs the credentials in this value to '*****:*****', which Uri cannot be relied on to parse.")]
    public string? Url { get; init; }

    public string? LastError { get; init; }

    public DateTimeOffset? LastUpdateAt { get; init; }

    public DateTimeOffset? LastUpdateStartedAt { get; init; }

    public DateTimeOffset? LastSuccessfulUpdateAt { get; init; }

    public bool? Enabled { get; init; }

    public bool? MirrorTriggerBuilds { get; init; }

    public bool? OnlyMirrorProtectedBranches { get; init; }

    public bool? MirrorOverwritesDivergedBranches { get; init; }

    public string? MirrorBranchRegex { get; init; }
}