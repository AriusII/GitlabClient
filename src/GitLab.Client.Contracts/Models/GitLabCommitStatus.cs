using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     A build status reported against a commit, as returned by the GitLab Commit statuses API
///     (<c>/projects/:id/repository/commits/:sha/statuses</c>).
/// </summary>
public sealed record GitLabCommitStatus
{
    public required long Id { get; init; }

    public required string Sha { get; init; }

    /// <summary>The branch or tag the status was reported against.</summary>
    public string? Ref { get; init; }

    /// <summary>
    ///     One of "pending", "running", "success", "failed", "canceled" or "skipped". Note the asymmetry with
    ///     <see cref="CreateCommitStatusRequest.State" />: GitLab reads the field as <c>state</c> and writes it
    ///     back as <c>status</c>.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>The label distinguishing this status from other systems' statuses. GitLab defaults it to "default".</summary>
    public string? Name { get; init; }

    /// <summary>Where the reporting system's own page for this build lives.</summary>
    public Uri? TargetUrl { get; init; }

    public string? Description { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? FinishedAt { get; init; }

    public bool? AllowFailure { get; init; }

    /// <summary>The total code coverage the reporting system measured, as a percentage.</summary>
    public double? Coverage { get; init; }

    public long? PipelineId { get; init; }

    /// <summary>The user the status was reported as.</summary>
    public GitLabUser? Author { get; init; }
}