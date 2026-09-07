namespace GitLab.Client.Models;

/// <summary>The body of <c>POST /groups/:id/export_relations</c>.</summary>
public sealed record ScheduleGroupRelationsExportRequest
{
    /// <summary>
    ///     Splits each relation into batches, which is what makes
    ///     <see cref="GitLabGroupRelationsExportStatus.Batches" /> populated and lets the download route be
    ///     called once per <c>batch_number</c>. GitLab defaults this to false.
    /// </summary>
    public bool? Batched { get; init; }
}