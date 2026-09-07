namespace GitLab.Client.Models;

/// <summary>
///     One batched background operation, as returned by
///     <c>GET /admin/batched_background_operations</c> and <c>GET /admin/batched_background_operations/:id</c>.
///     Operations are the successor mechanism to batched background migrations, driving arbitrary
///     background data changes rather than only schema migrations.
/// </summary>
public sealed record GitLabBatchedBackgroundOperation
{
    /// <summary>
    ///     The operation id, formatted by GitLab as <c>&lt;cluster&gt;:&lt;partition_id&gt;:&lt;id/uuid&gt;</c>
    ///     rather than a bare number - unlike the <c>id</c> path parameter used to look one operation up.
    /// </summary>
    public required string Id { get; init; }

    public int? Partition { get; init; }

    public required string JobClassName { get; init; }

    public string? TableName { get; init; }

    public string? ColumnName { get; init; }

    /// <summary>
    ///     Free text such as <c>queued</c>, <c>active</c>, <c>paused</c>, <c>stopped</c> or <c>finished</c>.
    ///     GitLab does not publish a closed vocabulary for this field, so it stays a plain string rather
    ///     than an enum a future status value could break deserialization of.
    /// </summary>
    public string? Status { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? FinishedAt { get; init; }

    /// <summary>When set, the operation is paused and will not resume before this instant.</summary>
    public DateTimeOffset? OnHoldUntil { get; init; }
}