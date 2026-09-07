namespace GitLab.Client.Models;

/// <summary>
///     Generated changelog markdown (<c>GET /projects/:id/repository/changelog</c>). The GET form
///     only generates the text; it never commits it to a file in the repository.
/// </summary>
public sealed record GitLabChangelog
{
    public required string Notes { get; init; }
}