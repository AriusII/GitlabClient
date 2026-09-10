namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/repository/commits/:sha/cherry_pick</c>.</summary>
public sealed record CherryPickCommitRequest
{
    /// <summary>The branch the commit is cherry-picked onto.</summary>
    public required string Branch { get; init; }

    /// <summary>
    ///     Runs the cherry-pick without committing anything, so a conflict surfaces as a
    ///     <see cref="Abstractions.Exceptions.GitLabApiException" /> instead of a rewritten branch.
    /// </summary>
    public bool? DryRun { get; init; }

    /// <summary>A custom commit message for the picked commit, replacing GitLab's generated one.</summary>
    public string? Message { get; init; }
}