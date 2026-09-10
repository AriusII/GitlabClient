namespace GitLab.Client.Models;

/// <summary>
///     An offline transfer export - the "write a migration to an object storage bucket, then import it
///     from there" half of direct transfer (<c>/offline_exports</c>).
/// </summary>
/// <remarks>
///     <para>
///         The pinned spec declares <em>no</em> response entity for the offline export routes (their
///         <c>200</c>/<c>201</c> responses carry no <c>content</c>), unlike the sibling
///         <c>POST /offline_imports</c>, which returns a <see cref="GitLabBulkImport" />. Every member
///         here is therefore optional and none is an enum: the names come from the operation's own request
///         schema plus the migration fields the family fixes, and an unexpected payload is skipped rather
///         than thrown on.
///     </para>
///     <para>
///         Treat unmapped fields as expected. If your instance returns something this does not model, the
///         escape hatch is <c>IGitLabApiConnection</c> with your own type.
///     </para>
/// </remarks>
public sealed record GitLabOfflineExport
{
    public long? Id { get; init; }

    /// <summary>
    ///     Where the export is in its lifecycle. Kept a bare string rather than
    ///     <see cref="GitLabOfflineExportStatus" /> because the spec never types the response field.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>The object storage bucket the export is written to.</summary>
    public string? Bucket { get; init; }

    /// <summary>The prefix the export is written under inside <see cref="Bucket" />.</summary>
    public string? ExportPrefix { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>Whether any part of the export recorded a failure.</summary>
    public bool? HasFailures { get; init; }
}