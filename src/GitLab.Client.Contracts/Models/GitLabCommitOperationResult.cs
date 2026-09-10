namespace GitLab.Client.Models;

/// <summary>
///     Result of cherry-picking or reverting a commit through the <c>*WithResultAsync</c> APIs. GitLab
///     answers with a commit after it changes a branch, but returns a compact <c>dry_run</c> payload when
///     the request uses <c>dry_run=true</c>.
/// </summary>
public sealed record GitLabCommitOperationResult
{
    /// <summary>The commit GitLab created, or <see langword="null" /> for a successful dry run.</summary>
    public GitLabCommit? Commit { get; init; }

    /// <summary>GitLab's dry-run outcome (normally <c>success</c>), or <see langword="null" /> after a commit.</summary>
    public string? DryRun { get; init; }

    /// <summary>Whether GitLab returned a dry-run outcome instead of creating a commit.</summary>
    public bool IsDryRun => DryRun is not null;
}