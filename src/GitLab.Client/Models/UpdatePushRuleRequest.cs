namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/push_rule</c> and <c>PUT /groups/:id/push_rule</c>. Every
///     member is optional; unset members are omitted from the payload rather than sent as null, so a
///     partial body leaves the rest of the push rule untouched.
/// </summary>
public sealed record UpdatePushRuleRequest
{
    /// <summary>Deny deleting a tag.</summary>
    public bool? DenyDeleteTag { get; init; }

    /// <summary>Restrict commits by author (email) to existing GitLab users.</summary>
    public bool? MemberCheck { get; init; }

    /// <summary>GitLab rejects any files that are likely to contain secrets.</summary>
    public bool? PreventSecrets { get; init; }

    /// <summary>All commit messages must match this RE2 regular expression.</summary>
    public string? CommitMessageRegex { get; init; }

    /// <summary>No commit message is allowed to match this RE2 regular expression.</summary>
    public string? CommitMessageNegativeRegex { get; init; }

    /// <summary>All branch names must match this RE2 regular expression.</summary>
    public string? BranchNameRegex { get; init; }

    /// <summary>All commit author emails must match this RE2 regular expression.</summary>
    public string? AuthorEmailRegex { get; init; }

    /// <summary>No committed file name is allowed to match this RE2 regular expression.</summary>
    public string? FileNameRegex { get; init; }

    /// <summary>Maximum file size in megabytes.</summary>
    public int? MaxFileSize { get; init; }

    /// <summary>Users can only push commits to this repository if the committer email is one of their own verified emails.</summary>
    public bool? CommitCommitterCheck { get; init; }

    /// <summary>Users can only push commits to this repository if the commit author name matches their GitLab account name.</summary>
    public bool? CommitCommitterNameCheck { get; init; }

    /// <summary>Reject a commit when it is not signed.</summary>
    public bool? RejectUnsignedCommits { get; init; }

    /// <summary>Reject a commit when it is not DCO certified.</summary>
    public bool? RejectNonDcoCommits { get; init; }
}