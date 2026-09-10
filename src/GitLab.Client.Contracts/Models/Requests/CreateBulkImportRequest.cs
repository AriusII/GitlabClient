namespace GitLab.Client.Models.Requests;

/// <summary>
///     The body of <c>POST /bulk_imports</c> - which instance to migrate from, and what to migrate.
/// </summary>
public sealed record CreateBulkImportRequest
{
    /// <summary>The source instance and the credential used to read from it. Never echoed back by GitLab.</summary>
    public required BulkImportConfiguration Configuration { get; init; }

    /// <summary>
    ///     The groups and projects to migrate. Migrating a single project needs an entry whose
    ///     <see cref="BulkImportEntityRequest.SourceType" /> is
    ///     <see cref="GitLabBulkImportEntitySourceType.ProjectEntity" />.
    /// </summary>
    public required IReadOnlyList<BulkImportEntityRequest> Entities { get; init; }
}