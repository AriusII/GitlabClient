namespace GitLab.Client.Models;

/// <summary>
///     The body of <c>POST /pipelines/:id/dependency_list_exports</c>. Both members are optional; GitLab
///     defaults to <see cref="GitLabPipelineDependencyListExportType.Sbom" /> and to not sending an
///     email.
/// </summary>
public sealed record CreatePipelineDependencyListExportRequest
{
    /// <summary>Email the requesting user once the export has finished generating.</summary>
    public bool? SendEmail { get; init; }

    /// <summary>The file format to produce.</summary>
    public GitLabPipelineDependencyListExportType? ExportType { get; init; }
}