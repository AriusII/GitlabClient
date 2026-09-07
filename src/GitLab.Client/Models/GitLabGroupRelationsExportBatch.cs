namespace GitLab.Client.Models;

/// <summary>
///     One batch of a batched group relations export, as reported by
///     <c>GET /groups/:id/export_relations/status</c>.
/// </summary>
public sealed record GitLabGroupRelationsExportBatch
{
    /// <summary>Where this batch is in its lifecycle.</summary>
    public GitLabGroupRelationExportBatchState? Status { get; init; }

    /// <summary>The 1-based batch index, which is what <c>batch_number</c> on the download route expects.</summary>
    public int? BatchNumber { get; init; }

    /// <summary>How many records this batch carries.</summary>
    public int? ObjectsCount { get; init; }

    /// <summary>Why the batch failed, when it did.</summary>
    public string? Error { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}