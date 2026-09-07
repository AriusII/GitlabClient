namespace GitLab.Client.Models;

/// <summary>Body for <c>POST /import/github/cancel</c>.</summary>
public sealed record CancelGitHubImportRequest
{
    /// <summary>
    ///     The numeric id of the GitLab project whose in-progress GitHub import should be cancelled.
    ///     Cancelling only works while the import is scheduled or started.
    /// </summary>
    public required long ProjectId { get; init; }
}