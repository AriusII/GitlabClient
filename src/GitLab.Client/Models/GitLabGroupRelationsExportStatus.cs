namespace GitLab.Client.Models;

/// <summary>
///     The progress of one relation of a group relations export
///     (<c>GET /groups/:id/export_relations/status</c>) - the read side of the "export relations, then
///     download them one relation at a time" flow.
/// </summary>
/// <remarks>
///     Every member is optional on purpose. GitLab reports a relation as soon as it is scheduled, long
///     before it has a batch count, an object count or an error, so nothing here is promised on the first
///     poll.
/// </remarks>
public sealed record GitLabGroupRelationsExportStatus
{
    /// <summary>The relation this entry describes - <c>issues</c>, <c>labels</c>, <c>milestones</c>, ...</summary>
    public string? Relation { get; init; }

    /// <summary>Where the relation's export is in its lifecycle.</summary>
    public GitLabGroupRelationExportState? Status { get; init; }

    /// <summary>Why the export failed, when it did.</summary>
    public string? Error { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>Whether this relation was exported in batches, which is what makes <see cref="Batches" /> non-empty.</summary>
    public bool? Batched { get; init; }

    /// <summary>How many batches the relation was split into.</summary>
    public int? BatchesCount { get; init; }

    /// <summary>How many records the relation holds in total, across every batch.</summary>
    public int? TotalObjectsCount { get; init; }

    /// <summary>Per-batch progress, present only for a batched export.</summary>
    public IReadOnlyList<GitLabGroupRelationsExportBatch>? Batches { get; init; }
}