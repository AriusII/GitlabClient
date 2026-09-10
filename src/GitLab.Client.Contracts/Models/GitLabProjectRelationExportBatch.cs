namespace GitLab.Client.Models;

/// <summary>
///     One batch of a batched project relations export. Download it with the relation name plus this
///     entry's <see cref="BatchNumber" />.
/// </summary>
public sealed record GitLabProjectRelationExportBatch
{
    /// <summary>How far along this batch is.</summary>
    public GitLabProjectRelationExportBatchState? Status { get; init; }

    /// <summary>The batch's own number, which is what the download route's <c>batch_number</c> takes. One-based.</summary>
    public int? BatchNumber { get; init; }

    /// <summary>How many records this batch holds.</summary>
    public int? ObjectsCount { get; init; }

    /// <summary>The failure message when <see cref="Status" /> is <see cref="GitLabProjectRelationExportBatchState.Failed" />.</summary>
    public string? Error { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}