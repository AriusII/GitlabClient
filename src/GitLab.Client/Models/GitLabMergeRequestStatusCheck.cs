namespace GitLab.Client.Models;

/// <summary>
///     One external status check as it applies to a specific merge request
///     (<c>GET /projects/:id/merge_requests/:iid/status_checks</c>) - the project-level check plus its current
///     result.
/// </summary>
public sealed record GitLabMergeRequestStatusCheck
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public Uri? ExternalUrl { get; init; }

    /// <summary>Free-form on the wire; GitLab reports <c>passed</c>, <c>failed</c> or <c>pending</c>.</summary>
    public string? Status { get; init; }
}