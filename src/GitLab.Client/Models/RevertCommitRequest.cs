namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/repository/commits/:sha/revert</c>.</summary>
public sealed record RevertCommitRequest
{
    /// <summary>The branch the revert commit is written to.</summary>
    public required string Branch { get; init; }

    /// <summary>Runs the revert without committing anything, turning a conflict into an error response.</summary>
    public bool? DryRun { get; init; }
}