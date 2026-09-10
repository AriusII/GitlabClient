namespace GitLab.Client.Models;

/// <summary>
///     One batched background migration, as returned by
///     <c>GET /admin/batched_background_migrations</c> and <c>GET /admin/batched_background_migrations/:id</c>.
///     These are Rails data migrations GitLab runs in small batches over time rather than in one
///     blocking database transaction.
/// </summary>
public sealed record GitLabBatchedBackgroundMigration
{
    /// <summary>The migration id. GitLab returns this as a numeric string, not a JSON number.</summary>
    public required string Id { get; init; }

    public required string JobClassName { get; init; }

    public string? TableName { get; init; }

    public string? ColumnName { get; init; }

    /// <summary>
    ///     Free text such as <c>active</c>, <c>paused</c> or <c>finished</c>. GitLab does not publish a
    ///     closed vocabulary for this field, so it stays a plain string rather than an enum a future
    ///     status value could break deserialization of.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>Percentage of batches completed, from 0 to 100.</summary>
    public double? Progress { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>A human-readable estimate such as <c>1 day</c>, not a machine-parseable duration.</summary>
    public string? EstimatedTimeRemaining { get; init; }
}