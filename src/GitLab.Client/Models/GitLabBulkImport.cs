namespace GitLab.Client.Models;

/// <summary>
///     A direct-transfer migration - GitLab's "import a group or project straight from another GitLab
///     instance" flow (<c>/bulk_imports</c>).
/// </summary>
/// <remarks>
///     The migration deliberately carries no credential: the access token for the source instance is
///     supplied once on <see cref="BulkImportConfiguration" /> and is never echoed back.
/// </remarks>
public sealed record GitLabBulkImport
{
    public required long Id { get; init; }

    /// <summary>Where the migration is in its lifecycle.</summary>
    public GitLabBulkImportStatus? Status { get; init; }

    /// <summary>What kind of instance the migration reads from. <c>gitlab</c> is the only value today.</summary>
    public string? SourceType { get; init; }

    /// <summary>The source instance the migration reads from.</summary>
    public Uri? SourceUrl { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    ///     Whether any entity of this migration recorded a failure. A migration can finish with this set:
    ///     GitLab skips the records it cannot import rather than aborting the run, so check it before
    ///     treating <see cref="GitLabBulkImportStatus.Finished" /> as a clean result.
    /// </summary>
    public bool? HasFailures { get; init; }
}