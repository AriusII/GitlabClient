namespace GitLab.Client.Models;

/// <summary>
///     A project's push rules (<c>/projects/:id/push_rule</c>) - server-side checks GitLab runs against
///     every push before accepting it (commit message shape, branch naming, secret detection, and so on).
///     <para>
///         A project has at most one push rule resource; GitLab answers <c>404</c> for the <c>GET</c> and
///         <c>DELETE</c> routes when none has been configured yet.
///     </para>
/// </summary>
public sealed record GitLabProjectPushRule
{
    public required long Id { get; init; }

    /// <summary>The project this push rule belongs to.</summary>
    public required long ProjectId { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>All commit messages must match this RE2 regular expression.</summary>
    public string? CommitMessageRegex { get; init; }

    /// <summary>No commit message is allowed to match this RE2 regular expression.</summary>
    public string? CommitMessageNegativeRegex { get; init; }

    /// <summary>All branch names must match this RE2 regular expression.</summary>
    public string? BranchNameRegex { get; init; }

    /// <summary>Whether deleting a tag via a push is denied.</summary>
    public bool? DenyDeleteTag { get; init; }

    /// <summary>Whether commit authors are restricted to existing GitLab users matched by their commit email.</summary>
    public bool? MemberCheck { get; init; }

    /// <summary>Whether GitLab rejects files that are likely to contain secrets.</summary>
    public bool? PreventSecrets { get; init; }

    /// <summary>All commit author emails must match this RE2 regular expression.</summary>
    public string? AuthorEmailRegex { get; init; }

    /// <summary>No committed file name is allowed to match this RE2 regular expression.</summary>
    public string? FileNameRegex { get; init; }

    /// <summary>The maximum file size allowed in a push, in megabytes.</summary>
    public int? MaxFileSize { get; init; }

    /// <summary>Whether a commit's committer email must be one of the pusher's own verified emails.</summary>
    public bool? CommitCommitterCheck { get; init; }

    /// <summary>Whether a commit's author name must match the pusher's GitLab account name.</summary>
    public bool? CommitCommitterNameCheck { get; init; }

    /// <summary>Whether unsigned commits are rejected.</summary>
    public bool? RejectUnsignedCommits { get; init; }

    /// <summary>Whether commits that are not DCO certified are rejected.</summary>
    public bool? RejectNonDcoCommits { get; init; }
}