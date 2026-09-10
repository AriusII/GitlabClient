namespace GitLab.Client.Models.Requests;

/// <summary>Body for <c>POST /projects/:id/export_relations</c>.</summary>
public sealed record ExportProjectRelationsRequest
{
    /// <summary>
    ///     Split each relation into batches rather than writing one file per relation. Batched exports are
    ///     downloaded a batch at a time, and report their progress in
    ///     <see cref="GitLabProjectRelationExportStatus.Batches" />.
    /// </summary>
    public bool? Batched { get; init; }
}