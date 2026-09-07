namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/statuses/:sha</c>.</summary>
public sealed record CreateCommitStatusRequest
{
    /// <summary>
    ///     One of "pending", "running", "success", "failed", "canceled" or "skipped". GitLab reads this as
    ///     <c>state</c> and echoes it back as <c>status</c> on <see cref="GitLabCommitStatus" />.
    /// </summary>
    public required string State { get; init; }

    /// <summary>The branch or tag the status belongs to.</summary>
    public string? Ref { get; init; }

    /// <summary>A link back to the reporting system's own page for this build.</summary>
    public Uri? TargetUrl { get; init; }

    public string? Description { get; init; }

    /// <summary>
    ///     A label distinguishing this status from other systems' statuses. <see cref="Name" /> and
    ///     <see cref="Context" /> are two spellings of the same field; GitLab defaults it to "default".
    /// </summary>
    public string? Name { get; init; }

    /// <inheritdoc cref="Name" />
    public string? Context { get; init; }

    /// <summary>The total code coverage, as a percentage.</summary>
    public double? Coverage { get; init; }

    /// <summary>Disambiguates which pipeline the status belongs to when several ran on the same SHA.</summary>
    public long? PipelineId { get; init; }
}