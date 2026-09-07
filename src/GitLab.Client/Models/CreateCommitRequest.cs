namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/repository/commits</c>: creates one commit that applies
///     every entry of <see cref="Actions" /> at once.
/// </summary>
public sealed record CreateCommitRequest
{
    /// <summary>The branch to commit to. Combine with <see cref="StartBranch" /> to create it on the way.</summary>
    public required string Branch { get; init; }

    /// <summary>The commit message.</summary>
    public required string CommitMessage { get; init; }

    /// <summary>The file operations this commit performs. GitLab rejects an empty array.</summary>
    public required IReadOnlyList<CommitAction> Actions { get; init; }

    /// <summary>Branch to start <see cref="Branch" /> from when it does not exist yet.</summary>
    public string? StartBranch { get; init; }

    /// <summary>Commit SHA to start <see cref="Branch" /> from when it does not exist yet.</summary>
    public string? StartSha { get; init; }

    /// <summary>The project the start branch or SHA lives in, for a cross-project commit.</summary>
    public string? StartProject { get; init; }

    /// <summary>Overrides the commit author's email address.</summary>
    public string? AuthorEmail { get; init; }

    /// <summary>Overrides the commit author's name.</summary>
    public string? AuthorName { get; init; }

    /// <summary>Whether to include <see cref="GitLabCommit.Stats" /> in the response. GitLab defaults to true.</summary>
    public bool? Stats { get; init; }

    /// <summary>Overwrites the branch with the new commit rather than appending to it.</summary>
    public bool? Force { get; init; }
}